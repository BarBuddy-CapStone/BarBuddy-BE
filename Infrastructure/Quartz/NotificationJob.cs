using Application.DTOs.Fcm;
using Application.DTOs.Notification;
using Application.Interfaces;
using Application.IService;
using Domain.Constants;
using Domain.CustomException;
using Domain.Entities;
using Domain.Enums;
using Domain.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using Quartz;

namespace Infrastructure.Quartz
{
    public class NotificationJob : IJob
    {
        private readonly IBookingService _bookingService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthentication _authen;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMemoryCache _cache;
        private readonly ILogger<NotificationJob> _logger;
        private readonly IFcmService _fcmService;

        public NotificationJob(
                    IBookingService bookingService,
                    IAuthentication authen, ILogger<NotificationJob> logger,
                    IHttpContextAccessor httpContextAccessor,
                    IMemoryCache cache,
                    IFcmService fcmService, IUnitOfWork unitOfWork)
        {
           
            _bookingService = bookingService;
            _authen = authen;
            _httpContextAccessor = httpContextAccessor;
            _cache = cache;
            _logger = logger;
            _fcmService = fcmService;
            _unitOfWork = unitOfWork;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                DateTimeOffset now = DateTimeOffset.Now;
                TimeSpan roundedTimeOfDay = TimeSpan.FromHours(now.TimeOfDay.Hours)
                                                   .Add(TimeSpan.FromMinutes(now.TimeOfDay.Minutes))
                                                   .Add(TimeSpan.FromSeconds(now.TimeOfDay.Seconds));
                TimeSpan roundedTwoHoursLater = new TimeSpan(
                                                    now.AddHours(2).TimeOfDay.Hours,
                                                    now.AddHours(2).TimeOfDay.Minutes,
                                                    now.AddHours(2).TimeOfDay.Seconds
                                                    );

                var getListBooking = await _bookingService.GetAllBookingByStsPending();
                var getBkStsPending = await _bookingService.GetAllBookingByStsPendingCus();

                // Check bk pendiing
                foreach (var booking in getBkStsPending)
                {

                    var cacheKey = $"{booking.BarName}_{booking.BookingDate.Date}_{booking.BookingTime}_{booking.BookingId}";
                    if (_cache.TryGetValue(cacheKey, out _))
                    {
                        //check booking có drink
                        if (booking.BookingDate.Date == now.Date &&
                        booking.BookingTime + TimeSpan.FromHours(booking.TimeSlot) == roundedTimeOfDay &&
                        booking.TotalPrice != null &&
                        booking.Status == (int)PrefixValueEnum.PendingBooking)
                        {
                            await _bookingService.UpdateBookingStatusJob(booking.AccountId, booking.BookingId, (int)PrefixValueEnum.Completed);
                            _cache.Remove(cacheKey);
                            var messages = string.Format(PrefixKeyConstant.BOOKING_DRINKS_COMPLETED_NOTI, booking.BarName);
                            
                            _logger.LogInformation("Đã hoàn thành đơn hàng đặt với đồ uống");
                            var bar = await _unitOfWork.BarRepository.GetByIdAsync(booking.BarId);
                            await _fcmService.CreateAndSendNotification(
                                new CreateNotificationRequest
                                {
                                    BarId = bar == null ? null : bar.BarId,
                                    MobileDeepLink = $"com.fptu.barbuddy://booking-detail/{booking.BookingId}",
                                    WebDeepLink = $"booking-detail/{booking.BookingId}",
                                    ImageUrl = bar == null ? null : bar.Images.Split(",")[0],
                                    IsPublic = false,
                                    Message = messages,
                                    Title = booking.BarName,
                                    Type = FcmNotificationType.BOOKING,
                                    SpecificAccountIds = new List<Guid> { booking.AccountId }
                                }
                            );
                        }
                        //check booking không có drink
                        else if (booking.BookingDate.Date == now.Date &&
                           booking.BookingTime + TimeSpan.FromHours(1) == roundedTimeOfDay &&
                           booking.TotalPrice == null &&
                           booking.Status == (int)PrefixValueEnum.PendingBooking)
                        {
                            await _bookingService.UpdateBookingStatusJob(booking.AccountId, booking.BookingId, (int)PrefixValueEnum.Cancelled);
                            var messages = string.Format(PrefixKeyConstant.BOOKING_CANCEL_NOTI_JOB, booking.BookingTime, booking.BookingDate ,booking.BarName);
                            
                            _cache.Remove(cacheKey);
                            var bar = await _unitOfWork.BarRepository.GetByIdAsync(booking.BarId);
                            await _fcmService.CreateAndSendNotification(
                                new CreateNotificationRequest
                                {
                                    BarId = bar == null ? null : bar.BarId,
                                    MobileDeepLink = $"com.fptu.barbuddy://booking-detail/{booking.BookingId}",
                                    WebDeepLink = $"booking-detail/{booking.BookingId}",
                                    ImageUrl = bar == null ? null : bar.Images.Split(",")[0],
                                    IsPublic = false,
                                    Message = messages,
                                    Title = booking.BarName,
                                    Type = FcmNotificationType.BOOKING,
                                    SpecificAccountIds = new List<Guid> { booking.AccountId }
                                }
                            );
                        }
                    }
                }

                foreach (var booking in getListBooking)
                {
                    // Nhắc nhở booking sắp tới
                    var cacheKey = $"{booking.BarName}_{booking.BookingDate.Date}_{booking.BookingTime}_{booking.BookingId}";
                    if (!_cache.TryGetValue(cacheKey, out _))
                    {
                        var messages = string.Format(PrefixKeyConstant.BOOKING_PENDING_NOTI, booking.BarName, booking.BookingDate.ToString("dd/MM/yyyy"), $"{booking.BookingTime.Hours:D2}:{booking.BookingTime.Minutes:D2}");
                        
                        _cache.Set(cacheKey, true, TimeSpan.FromHours(2 + booking.TimeSlot));

                        _logger.LogInformation($"Đã gửi thông báo cho tài khoản {booking.AccountId} với {getListBooking.Count()} đơn đặt !");
                        var bar = await _unitOfWork.BarRepository.GetByIdAsync(booking.BarId);
                        await _fcmService.CreateAndSendNotification(
                                new CreateNotificationRequest
                                {
                                    BarId = bar == null ? null : bar.BarId,
                                    MobileDeepLink = $"com.fptu.barbuddy://booking-detail/{booking.BookingId}",
                                    WebDeepLink = $"booking-detail/{booking.BookingId}",
                                    ImageUrl = bar == null ? null : bar.Images.Split(",")[0],
                                    IsPublic = false,
                                    Message = messages,
                                    Title = booking.BarName,
                                    Type = FcmNotificationType.BOOKING,
                                    SpecificAccountIds = new List<Guid> { booking.AccountId }
                                }
                            );
                    }
                    else
                    {
                        var staffId = _unitOfWork.AccountRepository
                                                .Get(filter: x => x.BarId.Equals(booking.BarId) &&
                                                                  x.Status == (int)PrefixValueEnum.Active)
                                                .Select(x => x.AccountId).ToList();

                        //Đến giờ
                        if (booking.BookingDate.Date == now.Date &&
                           booking.BookingTime == roundedTimeOfDay &&
                           booking.Status == (int)PrefixValueEnum.PendingBooking)
                        {
                            var messages = string.Format(PrefixKeyConstant.BOOKING_REMIND_NOTI, booking.BarName, booking.BookingDate.ToString("dd/MM/yyyy"), $"{booking.BookingTime.Hours:D2}:{booking.BookingTime.Minutes:D2}");
                            
                            _logger.LogInformation($"Đã gửi thông báo cho tài khoản {booking.AccountId} với {getListBooking.Count()} đơn đặt !");
                            var bar = await _unitOfWork.BarRepository.GetByIdAsync(booking.BarId);
                            await _fcmService.CreateAndSendNotification(
                                new CreateNotificationRequest
                                {
                                    BarId = bar == null ? null : bar.BarId,
                                    MobileDeepLink = $"com.fptu.barbuddy://booking-detail/{booking.BookingId}",
                                    WebDeepLink = $"booking-detail/{booking.BookingId}",
                                    ImageUrl = bar == null ? null : bar.Images.Split(",")[0],
                                    IsPublic = false,
                                    Message = messages,
                                    Title = booking.BarName,
                                    Type = FcmNotificationType.BOOKING,
                                    SpecificAccountIds = new List<Guid> { booking.AccountId }
                                }
                            );
                        }

                        // Khi slot hết giờ thì thông báo cho staff va` customer
                        var endTime = booking.BookingTime.Add(TimeSpan.FromHours(booking.TimeSlot));
                        var bookingEndDate = booking.BookingDate;

                        if (endTime.Hours < booking.BookingTime.Hours)
                        {
                            bookingEndDate = booking.BookingDate.AddDays(1);
                        }

                        if (bookingEndDate.Date == now.Date &&
                            endTime == roundedTimeOfDay &&
                            booking.Status == (int)PrefixValueEnum.PendingBooking)
                        {
                            var messagesCustomer = string.Format(PrefixKeyConstant.BOOKING_END_NOTI_CUSTOMER, booking.BarName, booking.BookingDate.ToString("dd/MM/yyyy"), $"{endTime.Hours:D2}:{endTime.Minutes:D2}");
                            var messagesStaff = string.Format(PrefixKeyConstant.BOOKING_END_NOTI_STAFF, booking.BarName, booking.BookingDate.ToString("dd/MM/yyyy"), $"{endTime.Hours:D2}:{endTime.Minutes:D2}");
                            _logger.LogInformation($"Đã gửi thông báo kết thúc timeslot cho tài khoản {booking.AccountId}");
                            var bar = await _unitOfWork.BarRepository.GetByIdAsync(booking.BarId);
                            await _fcmService.CreateAndSendNotification(
                                new CreateNotificationRequest
                                {
                                    BarId = bar?.BarId,
                                    MobileDeepLink = $"com.fptu.barbuddy://booking-detail/{booking.BookingId}",
                                    WebDeepLink = $"booking-detail/{booking.BookingId}",
                                    ImageUrl = bar?.Images.Split(",")[0],
                                    IsPublic = false,
                                    Message = messagesCustomer,
                                    Title = $"Hết giờ đặt chỗ - {booking.BarName}",
                                    Type = FcmNotificationType.BOOKING,
                                    SpecificAccountIds = new List<Guid> { booking.AccountId }
                                }
                            );

                            await _fcmService.CreateAndSendNotification(
                                new CreateNotificationRequest
                                {
                                    BarId = bar?.BarId,
                                    //MobileDeepLink = $"com.fptu.barbuddy://booking-detail/{booking.BookingId}",
                                    WebDeepLink = $"staff/table-registration-detail/{booking.BookingId}",
                                    ImageUrl = bar?.Images.Split(",")[0],
                                    IsPublic = false,
                                    Message = messagesStaff,
                                    Title = $"Hết giờ đặt chỗ - {booking.BarName}",
                                    Type = FcmNotificationType.BOOKING,
                                    SpecificAccountIds = staffId
                                }
                            );
                        }
                    }
                }
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                throw new CustomException.InternalServerErrorException("Lỗi hệ thống !");
            }
        }
    }
}
