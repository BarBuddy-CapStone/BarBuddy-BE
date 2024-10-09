using Application.DTOs.BookingTable;
using Application.DTOs.Table;
using Application.DTOs.TableType;
using Application.Interfaces;
using Application.IService;
using AutoMapper;
using Domain.Constants;
using Domain.CustomException;
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
                if (TimeHelper.ConvertToUtcPlus7(request.Date.Date) < TimeHelper.ConvertToUtcPlus7(DateTimeOffset.Now.Date))
                {
                    throw new CustomException.InvalidDataException("Invalid data");
                }

                var data = await _unitOfWork.TableRepository.GetAsync(
                                                filter: x => x.BarId.Equals(request.BarId)
                                                          && x.TableTypeId.Equals(request.TableTypeId)
                                                          && x.IsDeleted == PrefixKeyConstant.FALSE,
                                                includeProperties: "BookingTables.Booking,TableType,Bar");

                if (data.IsNullOrEmpty())
                {
                    throw new CustomException.InvalidDataException("Invalid data");
                }

                var getOne = data.Where(x => x.BookingTables != null && x.BookingTables.Any()).FirstOrDefault();


                var response = _mapper.Map<FilterTableTypeReponse>(getOne.TableType);

                response.BookingTables = new List<FilterBkTableResponse>
                {
                    new FilterBkTableResponse
                    {
                        ReservationDate = getOne.BookingTables.FirstOrDefault().ReservationDate,
                        ReservationTime = getOne.BookingTables.FirstOrDefault().ReservationTime,
                        Tables = data.Select(bt => new FilterTableResponse
                                    {
                                        TableId = bt.TableId,
                                        TableName = bt.TableName,
                                        Status = bt.BookingTables != null && bt.BookingTables.Any()
                                                ? bt.BookingTables.FirstOrDefault()?.Booking.Status ?? 0
                                                : 0
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
                                                            && x.TableId.Equals(request.TableId))
                                        .FirstOrDefault();
            if (tableIsExist == null)
            {
                throw new CustomException.DataNotFoundException("Không tìm thấy table trong quán bar!");
            }

            var cacheKey = $"{request.BarId}_{request.TableId}";
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
                IsHeld = true,
                HoldExpiry = DateTimeOffset.Now.AddMinutes(5)
            };

            cacheEntry[request.TableId] = tableHoldInfo;

            await _bookingHub.HoldTable(request.TableId);

            _memoryCache.Set(cacheKey, cacheEntry, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });

            return tableHoldInfo;
        }

        public async Task<TableHoldInfo> ReleaseTable(TablesRequest request)
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

            var cacheKey = $"{request.BarId}_{request.TableId}";

            var cacheEntry = _memoryCache.GetOrCreate(cacheKey, entry =>
            {
                return new Dictionary<Guid, TableHoldInfo>();
            });

            var tableHoldInfo = new TableHoldInfo();
            if (cacheEntry.ContainsKey(request.TableId)
                && cacheEntry[request.TableId].AccountId.Equals(accountId)
                && cacheEntry[request.TableId].HoldExpiry <= DateTime.Now)
            {
                tableHoldInfo.AccountId = Guid.Empty;   
                tableHoldInfo.IsHeld = false;
                
            }
            cacheEntry[request.TableId] = tableHoldInfo;
            await _bookingHub.ReleaseTable(request.TableId);
            _memoryCache.Set(request.TableId, cacheEntry);
            return tableHoldInfo;
        }
    }
}
