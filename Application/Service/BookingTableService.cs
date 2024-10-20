using Application.Common;
using Application.DTOs.BookingTable;
using Application.DTOs.Table;
using Application.DTOs.TableType;
using Application.Interfaces;
using Application.IService;
using AutoMapper;
using Azure.Core;
using Domain.Constants;
using Domain.CustomException;
using Domain.Entities;
using Domain.Enums;
using Domain.IRepository;
using Domain.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Application.Service
{
    public class BookingTableService : IBookingTableService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _memoryCache;
        private readonly IBookingHubService _bookingHub;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<BookingTableService> _logger;
        private readonly IAuthentication _authentication;

        public BookingTableService
                (IUnitOfWork unitOfWork, IMapper mapper,
                IMemoryCache memoryCache, IBookingHubService bookingHub,
                ILogger<BookingTableService> logger, IHttpContextAccessor httpContextAccessor,
                IAuthentication authentication)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _memoryCache = memoryCache;
            _bookingHub = bookingHub;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _authentication = authentication;
        }

        public async Task<FilterTableTypeReponse> FilterTableTypeReponse(FilterTableDateTimeRequest request)
        {
            try
            {

                var getOneBar = _unitOfWork.BarRepository.GetByID(request.BarId);

                var requestDate = TimeHelper.ConvertToUtcPlus7(request.Date.Date);
                var currentDate = TimeHelper.ConvertToUtcPlus7(DateTimeOffset.Now.Date);
                var currentTime = TimeHelper.ConvertToUtcPlus7(DateTimeOffset.UtcNow);

                Utils.ValidateOpenCloseTime(requestDate, request.Time, getOneBar.StartTime, getOneBar.EndTime);

                var data = await _unitOfWork.TableRepository.GetAsync(
                                                filter: x => x.BarId.Equals(request.BarId)
                                                          && x.TableTypeId.Equals(request.TableTypeId)
                                                          && x.IsDeleted == PrefixKeyConstant.FALSE,
                                                includeProperties: "BookingTables.Booking,TableType,Bar");



                if (data.IsNullOrEmpty())
                {
                    throw new CustomException.InvalidDataException("Data not found !");
                }

                var getOne = data.Where(x => x.BookingTables != null && x.BookingTables.Any()).FirstOrDefault();


                var response = _mapper.Map<FilterTableTypeReponse>(getOne.TableType);

                response.BookingTables = new List<FilterBkTableResponse>
                {
                    new FilterBkTableResponse
                    {
                        ReservationDate = request.Date,
                        ReservationTime = request.Time,
                        Tables = data.Select(bt => new FilterTableResponse
                                    {
                                        TableId = bt.TableId,
                                        TableName = bt.TableName,
                                        Status = bt.BookingTables != null
                                                    && bt.BookingTables
                                                    .Where(x => x.ReservationDate.Date.Equals(request.Date.Date)
                                                            && x.ReservationTime == request.Time)
                                                    .Any()
                                                    ? bt.BookingTables
                                                    .Where(x => x.ReservationDate.Date.Equals(request.Date.Date)
                                                            && x.ReservationTime == request.Time)
                                                    .FirstOrDefault()?.Booking.Status ?? (int)PrefixValueEnum.Peding
                                                    : (int)PrefixValueEnum.Cancelled
                                    }).ToList()
                    }
                };

                return response;
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                throw new CustomException.InternalServerErrorException("Đã xảy ra lỗi");
            }
        }

        public async Task<TableHoldInfo> HoldTable(TablesRequest request)
        {
            var accountId = _authentication.GetUserIdFromHttpContext(_httpContextAccessor.HttpContext);
            var tableIsExist = _unitOfWork.TableRepository
                                        .Get(filter: x => x.BarId.Equals(request.BarId)
                                                            && x.TableId.Equals(request.TableId),
                                                            includeProperties: "Bar")
                                        .FirstOrDefault();

            if (tableIsExist == null)
            {
                throw new CustomException.DataNotFoundException("Không tìm thấy table trong quán bar!");
            }

            Utils.ValidateOpenCloseTime(request.Date, request.Time, tableIsExist.Bar.StartTime, tableIsExist.Bar.EndTime);

            var cacheKey = $"{request.BarId}_{request.TableId}_{request.Date.Date.Date}_{request.Time}";
            var cacheEntry = _memoryCache.GetOrCreate(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);

                entry.RegisterPostEvictionCallback((key, value, reason, state) =>
                {
                    var tableDictionary = value as Dictionary<Guid, TableHoldInfo>;

                    if (tableDictionary != null)
                    {
                        foreach (var tableEntry in tableDictionary)
                        {
                            var tableId = tableEntry.Key;
                            var tableHoldInfo = tableEntry.Value;

                            if (tableHoldInfo.HoldExpiry < DateTimeOffset.Now)
                            {
                                tableHoldInfo.IsHeld = false;
                                tableHoldInfo.AccountId = Guid.Empty;
                            }

                            _logger.LogInformation($"Cache entry {key} for TableId {tableId} was removed because {reason}. Table is now released.");
                        }
                    }
                });

                return new Dictionary<Guid, TableHoldInfo>();
            });

            if (cacheEntry.ContainsKey(request.TableId)
                && cacheEntry[request.TableId].IsHeld
                && !cacheEntry[request.TableId].AccountId.Equals(accountId))
            {
                throw new CustomException.InvalidDataException($"Bàn {request.TableId} đã bị giữ bởi người khác.");
            }

            var tableHoldInfo = new TableHoldInfo
            {
                AccountId = accountId,
                TableId = tableIsExist.TableId,
                TableName = tableIsExist.TableName,
                IsHeld = true,
                HoldExpiry = DateTimeOffset.Now.AddMinutes(5),
                Date = request.Date,
                Time = request.Time,
            };

            cacheEntry[request.TableId] = tableHoldInfo;

            var bkHubResponse = _mapper.Map<BookingHubResponse>(tableHoldInfo);

            await _bookingHub.HoldTable(bkHubResponse);

            _memoryCache.Set(cacheKey, cacheEntry, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });

            return tableHoldInfo;
        }

        public Task<List<TableHoldInfo>> HoldTableList(Guid barId, DateTimeRequest request)
        {
            var tableIsExist = _unitOfWork.TableRepository
                                        .Get(filter: x => x.BarId.Equals(barId)
                                                            && x.IsDeleted == PrefixKeyConstant.FALSE);

            List<TableHoldInfo> tableHolds = new List<TableHoldInfo>();
            foreach (var table in tableIsExist)
            {
                var cacheKey = $"{barId}_{table.TableId}_{request.Date.Date.Date.DayOfWeek}_{request.Time}";
                var cacheEntry = _memoryCache.GetOrCreate(cacheKey, entry =>
                {
                    return new Dictionary<Guid, TableHoldInfo>();
                });

                foreach (var tbHeld in cacheEntry)
                {
                    if (tbHeld.Key.ToString().Contains(table.TableId.ToString()))
                    {
                        tableHolds.Add(tbHeld.Value);
                    }
                }
            }

            return Task.FromResult(tableHolds.Where(x => x.IsHeld == true).ToList());
        }

        public async Task ReleaseTable(TablesRequest request)
        {

            var accountId = _authentication.GetUserIdFromHttpContext(_httpContextAccessor.HttpContext);
            var tableIsExist = _unitOfWork.TableRepository
                                        .Get(filter: x => x.BarId.Equals(request.BarId)
                                                            && x.TableId.Equals(request.TableId))
                                        .FirstOrDefault();
            if (tableIsExist == null)
            {
                throw new CustomException.DataNotFoundException("Không tìm thấy table trong quán bar!");
            }

            var cacheKey = $"{request.BarId}_{request.TableId}_{request.Date.Date.Date}_{request.Time}";

            var cacheEntry = _memoryCache.GetOrCreate(cacheKey, entry =>
            {
                return new Dictionary<Guid, TableHoldInfo>();
            });

            if (cacheEntry.ContainsKey(request.TableId)
                && cacheEntry[request.TableId].AccountId.Equals(accountId)
                && cacheEntry[request.TableId].HoldExpiry >= DateTime.Now
                && cacheEntry[request.TableId].Date.Date.Equals(request.Date.Date)
                && cacheEntry[request.TableId].Time.Equals(request.Time))
            {
                _memoryCache.Remove(cacheKey);
            }
            await _bookingHub.ReleaseListTablee(request.BarId);
        }

        public async Task ReleaseListTable(ReleaseListTableRequest request)
        {
            var accountId = _authentication.GetUserIdFromHttpContext(_httpContextAccessor.HttpContext);

            foreach (var table in request.Table)
            {
                var tableExist = await _unitOfWork.TableRepository.GetByIdAsync(table.TableId);
                if (tableExist == null)
                {
                    throw new CustomException.InvalidDataException("Không hợp lệ");
                }
                var cacheKey = $"{request.BarId}_{table.TableId}_{request.Date.Date.Date}_{table.Time}";
                if (_memoryCache.TryGetValue(cacheKey, out Dictionary<Guid, TableHoldInfo>? cacheTbHold))
                {
                    if (cacheTbHold.TryGetValue(table.TableId, out var tbHold))
                    {
                        if (tbHold.Date.Date.Date.Equals(request.Date.Date.Date)
                            && tbHold.Time.Equals(table.Time)
                            && tbHold.AccountId.Equals(accountId))
                        {
                            _memoryCache.Remove(cacheKey);
                            await _bookingHub.ReleaseListTablee(request.BarId);
                        }
                    }
                };
            }
        }
    }
}