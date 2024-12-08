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
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using System.Threading;

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
        //private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
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
                var getDayOfWeek = (int)requestDate.DayOfWeek;

                var getTimeOfBar = _unitOfWork.BarTimeRepository
                                                    .Get(filter: x => x.BarId.Equals(request.BarId)
                                                            && (x.DayOfWeek == getDayOfWeek || x.DayOfWeek == (getDayOfWeek + 1))).ToList();

                if (getDayOfWeek == null)
                {
                    throw new CustomException.DataNotFoundException("Không tìm thấy khung giờ của quán Bar !");
                }

                if (getTimeOfBar.Count > 1)
                {
                    var timesToRemove = new List<BarTime>();
                    foreach (var time in getTimeOfBar)
                    {
                        if (time.DayOfWeek != getDayOfWeek)
                        {
                            timesToRemove.Add(time); 
                        }
                    }
                    foreach (var time in timesToRemove)
                    {
                        getTimeOfBar.Remove(time);
                    }
                    Utils.ValidateOpenCloseTime(requestDate, request.Time, getTimeOfBar);
                }
                else if (getTimeOfBar.Any())
                {
                    if (getTimeOfBar[0].StartTime > getTimeOfBar[0].EndTime && request.Time < getTimeOfBar[0].StartTime) {
                        requestDate = requestDate.AddDays(1);
                        request.Date = request.Date.AddDays(1);
                    } else
                    {
                        Utils.ValidateOpenCloseTime(requestDate, request.Time, getTimeOfBar);
                    }
                }
                else
                {
                    throw new CustomException.DataNotFoundException("Không tìm thấy khung giờ của quán Bar !");
                }

                var data = await _unitOfWork.TableRepository.GetAsync(
                                                filter: x => x.TableType.BarId.Equals(request.BarId)
                                                            && x.TableTypeId.Equals(request.TableTypeId)
                                                          && x.IsDeleted == PrefixKeyConstant.FALSE &&
                                                          x.TableType.IsDeleted == PrefixKeyConstant.FALSE,
                                                includeProperties: "BookingTables.Booking,TableType.Bar.BarTimes");

                if (data.IsNullOrEmpty())
                {
                    throw new CustomException.InvalidDataException("Data not found !");
                }

                var getOne = data.FirstOrDefault();


                var response = _mapper.Map<FilterTableTypeReponse>(getOne?.TableType);

                response.BookingTables = new List<FilterBkTableResponse>
                {
                    new FilterBkTableResponse
                    {
                        ReservationDate = request.Date,
                        ReservationTime = request.Time,
                        Tables = data.Select(bt =>
                        {
                            var matchingBooking = bt.BookingTables?
                                .FirstOrDefault(x => x.Booking.BookingDate.Date == requestDate && x.Booking.BookingTime == request.Time);

                            return new FilterTableResponse
                            {
                                TableId = bt.TableId,
                                TableName = bt.TableName,
                                Status = matchingBooking?.Booking.Status ?? (int)PrefixValueEnum.Cancelled
                            };
                        }).ToList()
                    }
                };

                return response;
            }
            catch (CustomException.DataNotFoundException ex)
            {
                throw new CustomException.DataNotFoundException(ex.Message);
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
                                        .Get(filter: x => x.TableType.BarId.Equals(request.BarId)
                                                            && x.TableId.Equals(request.TableId)
                                                            && x.IsDeleted == PrefixKeyConstant.FALSE &&
                                                          x.TableType.IsDeleted == PrefixKeyConstant.FALSE,
                                                            includeProperties: "TableType.Bar")
                                        .FirstOrDefault();
            if(tableIsExist == null)
            {
                throw new CustomException.DataNotFoundException("Không tìm thấy bàn trong quán Bar, vui lòng thử lại !");
            }

            try
            {
                var currentHeldTables = await HoldTableList(request.BarId, new DateTimeRequest
                {
                    Date = request.Date,
                    Time = request.Time
                });

                if(currentHeldTables.Any())
                {
                    var currentTable = currentHeldTables.First();
                    if(currentTable.Date.Date != request.Date.Date)
                    {
                        throw new CustomException.InvalidDataException(
                        $"Bạn đã giữ bàn cho ngày {currentTable.Date.ToString("dd/MM/yyyy")}. " +
                        $"Không thể giữ bàn cho ngày {request.Date.ToString("dd/MM/yyyy")}");
                    }
                }

                if (currentHeldTables.Count > 6)
                {
                    throw new CustomException.InvalidDataException("Bạn chỉ được phép giữ tối đa 5 bàn cùng lúc.");
                }
                var getTimeOfBar = _unitOfWork.BarTimeRepository
                                            .Get(filter: x => x.BarId.Equals(tableIsExist.TableType.BarId)
                                                        && x.DayOfWeek == (int)request.Date.DayOfWeek).ToList();

                if (getTimeOfBar == null || !getTimeOfBar.Any())
                {
                    throw new CustomException.DataNotFoundException("Không tìm thấy khung giờ trong quán bar!");
                }

                Utils.ValidateOpenCloseTime(request.Date, request.Time, getTimeOfBar);

                var cacheKey = $"{request.BarId}_{request.TableId}_{request.Date.Date.Date}_{request.Time}";
                var cacheEntry = _memoryCache.GetOrCreate(cacheKey, entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);

                    entry.RegisterPostEvictionCallback(async (key, value, reason, state) =>
                    {
                        var tableDictionary = value as Dictionary<Guid, TableHoldInfo>;
                        if (tableDictionary != null)
                        {
                            foreach (var tableEntry in tableDictionary)
                            {
                                var tableId = tableEntry.Key;
                                var tableHoldInfo = tableEntry.Value;

                                if (reason == EvictionReason.Expired || reason == EvictionReason.Removed)
                                {
                                    tableHoldInfo.IsHeld = false;
                                    tableHoldInfo.AccountId = Guid.Empty;

                                    var bkHubResponse = _mapper.Map<BookingHubResponse>(tableHoldInfo);
                                    await _bookingHub.ReleaseTable(bkHubResponse);

                                    _logger.LogInformation($"Cache entry {key} for TableId {tableId} was removed because {reason}. Table is now released.");
                                }
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
                    HoldExpiry = DateTimeOffset.Now.AddMinutes(1),
                    Date = request.Date,
                    Time = request.Time,
                };

                cacheEntry[request.TableId] = tableHoldInfo;

                var bkHubResponse = _mapper.Map<BookingHubResponse>(tableHoldInfo);

                await _bookingHub.HoldTable(bkHubResponse);

                _memoryCache.Set(cacheKey, cacheEntry, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
                });

                return tableHoldInfo;

            }catch(CustomException.InternalServerErrorException ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message);
            } 

        }

        public Task<List<TableHoldInfo>> HoldTableList(Guid barId, DateTimeRequest request)
        {
            var tableIsExist = _unitOfWork.TableRepository
                                        .Get(filter: x => x.TableType.BarId.Equals(barId)
                                                            && x.IsDeleted == PrefixKeyConstant.FALSE &&
                                                          x.TableType.IsDeleted == PrefixKeyConstant.FALSE);
            var accountId = _authentication.GetUserIdFromHttpContext(_httpContextAccessor.HttpContext);
            List<TableHoldInfo> tableHolds = new List<TableHoldInfo>();
            foreach (var table in tableIsExist)
            {
                var cacheKey = $"{barId}_{table.TableId}_{request.Date.Date.Date}_{request.Time}";
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
                                        .Get(filter: x => x.TableType.BarId.Equals(request.BarId)
                                                            && x.TableId.Equals(request.TableId) &&
                                                          x.TableType.IsDeleted == PrefixKeyConstant.FALSE,
                                                          includeProperties: "TableType")
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

            var bkHubResponse = _mapper.Map<BookingHubResponse>(cacheEntry[request.TableId]);
            await _bookingHub.ReleaseTable(bkHubResponse);
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
                            var mapper = _mapper.Map<BookingHubResponse>(cacheTbHold[table.TableId]);
                            mapper.BarId = request.BarId;
                            mapper.AccountId = Guid.Empty;
                            await _bookingHub.ReleaseListTablee(mapper);
                        }
                    }
                };
            }
        }
    }
}