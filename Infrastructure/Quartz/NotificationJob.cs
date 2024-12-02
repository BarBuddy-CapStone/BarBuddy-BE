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
        private readonly INotificationService _notificationService;
        private readonly IBookingService _bookingService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthentication _authen;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMemoryCache _cache;
        private readonly ILogger<NotificationJob> _logger;
        private readonly IFcmService _fcmService;

        public NotificationJob(INotificationService notificationService,
                    IBookingService bookingService,
                    IAuthentication authen, ILogger<NotificationJob> logger,
                    IHttpContextAccessor httpContextAccessor,
                    IMemoryCache cache,
                    IFcmService fcmService, IUnitOfWork unitOfWork)
        {
            _notificationService = notificationService;
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
                            var creNotiRequest = new NotificationRequest
                            {
                                BarId = (Guid)booking.BarId,
                                Title = booking.BarName,
                                Message = messages
                            };
                            // await _notificationService.CreateNotificationAllCustomer(booking.AccountId, creNotiRequest);
                            _logger.LogInformation("Đã hoàn thành đơn hàng đặt với đồ uống");
                            var bar = await _unitOfWork.BarRepository.GetByIdAsync(booking.BarId);
                            await _fcmService.CreateAndSendNotificationToCustomer(
                                new CreateNotificationRequest
                                {
                                    BarId = bar == null ? null : bar.BarId,
                                    DeepLink = $"com.fptu.barbuddy://booking-detail/{booking.BookingId}",
                                    ImageUrl = bar == null ? null : bar.Images.Split(",")[0],
                                    IsPublic = false,
                                    Message = messages,
                                    Title = booking.BarName,
                                    Type = FcmNotificationType.BOOKING
                                }, booking.AccountId
                            );
                        }
                        //check booking không có drink
                        else if (booking.BookingDate.Date == now.Date &&
                           booking.BookingTime + TimeSpan.FromHours(1) == roundedTimeOfDay &&
                           booking.TotalPrice == null &&
                           booking.Status == (int)PrefixValueEnum.PendingBooking)
                        {
                            await _bookingService.UpdateBookingStatusJob(booking.AccountId, booking.BookingId, (int)PrefixValueEnum.Cancelled);
                            var messages = string.Format(PrefixKeyConstant.BOOKING_CANCEL_NOTI_JOB, booking.BarName);
                            var creNotiRequest = new NotificationRequest
                            {
                                BarId = (Guid)booking.BarId,
                                Title = booking.BarName,
                                Message = messages
                            };
                            // await _notificationService.CreateNotificationAllCustomer(booking.AccountId, creNotiRequest);
                            _cache.Remove(cacheKey);
                            _logger.LogInformation("Đã hoàn thành đơn hàng đặt với đồ uống");
                            var bar = await _unitOfWork.BarRepository.GetByIdAsync(booking.BarId);
                            await _fcmService.CreateAndSendNotificationToCustomer(
                                new CreateNotificationRequest
                                {
                                    BarId = bar == null ? null : bar.BarId,
                                    DeepLink = $"com.fptu.barbuddy://booking-detail/{booking.BookingId}",
                                    ImageUrl = bar == null ? null : bar.Images.Split(",")[0],
                                    IsPublic = false,
                                    Message = messages,
                                    Title = booking.BarName,
                                    Type = FcmNotificationType.BOOKING
                                }, booking.AccountId
                            );
                        }
                    }
                }
                //if (getListBooking.IsNullOrEmpty())
                //{
                //    _logger.LogInformation("Không có danh sách booking nào đang chờ !");
                //}
                

                foreach (var booking in getListBooking)
                {
                    // Nhắc nhở booking sắp tới
                    var cacheKey = $"{booking.BarName}_{booking.BookingDate.Date}_{booking.BookingTime}_{booking.BookingId}";
                    if (!_cache.TryGetValue(cacheKey, out _))
                    {
                        var messages = string.Format(PrefixKeyConstant.BOOKING_PENDING_NOTI, booking.BarName, booking.BookingDate.ToString("dd/MM/yyyy"), $"{booking.BookingTime.Hours:D2}:{booking.BookingTime.Minutes:D2}");
                        var creNotiRequest = new NotificationRequest
                        {
                            BarId = (Guid)booking.BarId,
                            Title = booking.BarName,
                            Message = messages
                        };
                        // await _notificationService.CreateNotificationAllCustomer(booking.AccountId, creNotiRequest);
                        _cache.Set(cacheKey, true, TimeSpan.FromHours(2 + booking.TimeSlot));

                        _logger.LogInformation($"Đã gửi thông báo cho tài khoản {booking.AccountId} với {getListBooking.Count()} đơn đặt !");
                        var bar = await _unitOfWork.BarRepository.GetByIdAsync(booking.BarId);
                        await _fcmService.CreateAndSendNotificationToCustomer(
                            new CreateNotificationRequest
                            {
                                BarId = bar == null ? null : bar.BarId,
                                DeepLink = $"com.fptu.barbuddy://booking-detail/{booking.BookingId}",
                                ImageUrl = bar == null ? null : bar.Images.Split(",")[0],
                                IsPublic = false,
                                Message = messages,
                                Title = booking.BarName,
                                Type = FcmNotificationType.BOOKING
                            }, booking.AccountId
                        );
                    }
                    else
                    {
                        //Đến giờ
                        if (booking.BookingDate.Date == now.Date &&
                           booking.BookingTime == roundedTimeOfDay &&
                           booking.Status == (int)PrefixValueEnum.PendingBooking)
                        {
                            var messages = string.Format(PrefixKeyConstant.BOOKING_REMIND_NOTI, booking.BarName, booking.BookingDate.ToString("dd/MM/yyyy"), $"{booking.BookingTime.Hours:D2}:{booking.BookingTime.Minutes:D2}");
                            var creNotiRequest = new NotificationRequest
                            {
                                BarId = (Guid)booking.BarId,
                                Title = booking.BarName,
                                Message = messages
                            };
                            // await _notificationService.CreateNotificationAllCustomer(booking.AccountId, creNotiRequest);
                            _logger.LogInformation($"Đã gửi thông báo cho tài khoản {booking.AccountId} với {getListBooking.Count()} đơn đặt !");
                            var bar = await _unitOfWork.BarRepository.GetByIdAsync(booking.BarId);
                            await _fcmService.CreateAndSendNotificationToCustomer(
                                new CreateNotificationRequest
                                {
                                    BarId = bar == null ? null : bar.BarId,
                                    DeepLink = $"com.fptu.barbuddy://booking-detail/{booking.BookingId}",
                                    ImageUrl = bar == null ? null : bar.Images.Split(",")[0],
                                    IsPublic = false,
                                    Message = messages,
                                    Title = booking.BarName,
                                    Type = FcmNotificationType.BOOKING
                                }, booking.AccountId
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
