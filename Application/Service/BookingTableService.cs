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
using Microsoft.Extensions.Primitives;
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
                    throw new CustomException.InvalidDataException("Không tìm thấy !");
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
                                Status = matchingBooking?.Booking.Status ?? (int)PrefixValueEnum.Cancelled,
                                MinimumGuest = bt.TableType.MinimumGuest,
                                MaximumGuest = bt.TableType.MaximumGuest,
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
                
                // Tạo CancellationTokenSource với timeout
                var cts = new CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromMinutes(5));

                var hasSignalRSent = false; // Flag để kiểm soát việc gửi SignalR

                var cacheEntry = _memoryCache.GetOrCreate(cacheKey, entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);

                    // Đăng ký token để handle khi cache expired
                    entry.AddExpirationToken(new CancellationChangeToken(cts.Token));

                    // Register callback khi cache bị remove
                    entry.RegisterPostEvictionCallback(async (key, value, reason, state) =>
                    {
                        if (hasSignalRSent) return; // Kiểm tra nếu đã gửi SignalR rồi thì bỏ qua

                        var tableDictionary = value as Dictionary<Guid, TableHoldInfo>;
                        if (tableDictionary != null && tableDictionary.TryGetValue(request.TableId, out var tableHoldInfo))
                        {
                            tableHoldInfo.IsHeld = false;
                            tableHoldInfo.AccountId = Guid.Empty;

                            try
                            {
                                var bkHubResponse = _mapper.Map<BookingHubResponse>(tableHoldInfo);
                                bkHubResponse.TableName = tableIsExist.TableName;
                                await _bookingHub.ReleaseTable(bkHubResponse);
                                hasSignalRSent = true; // Đánh dấu đã gửi SignalR
                                _logger.LogInformation($"Cache entry {key} for TableId {request.TableId} was removed because {reason}. Table is now released.");
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error sending SignalR notification on cache eviction");
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
                bkHubResponse.TableName = tableIsExist.TableName;
                await _bookingHub.HoldTable(bkHubResponse);

                _memoryCache.Set(cacheKey, cacheEntry, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });

                // Đăng ký callback khi token bị cancel (timeout)
                cts.Token.Register(async () =>
                {
                    if (hasSignalRSent) return; // Kiểm tra nếu đã gửi SignalR rồi thì bỏ qua

                    try 
                    {
                        _memoryCache.Remove(cacheKey);
                        var bkHubResponse = _mapper.Map<BookingHubResponse>(tableHoldInfo);
                        bkHubResponse.TableName = tableIsExist.TableName;
                        await _bookingHub.ReleaseTable(bkHubResponse);
                        hasSignalRSent = true; // Đánh dấu đã gửi SignalR
                        _logger.LogInformation($"Table {request.TableId} was released due to timeout");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error handling table release on timeout");
                    }
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
            bkHubResponse.TableName = tableIsExist.TableName;
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
                            mapper.TableName = tableExist.TableName;
                            await _bookingHub.ReleaseListTablee(mapper);
                        }
                    }
                };
            }
        }
    }
}