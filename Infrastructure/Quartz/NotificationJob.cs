using Application.DTOs.Notification;
using Application.Interfaces;
using Application.IService;
using Domain.Constants;
using Domain.CustomException;
using Domain.Entities;
using Domain.Enums;
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
        private readonly IAuthentication _authen;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMemoryCache _cache;
        private readonly ILogger<NotificationJob> _logger;

        public NotificationJob(INotificationService notificationService,
                    IBookingService bookingService,
                    IAuthentication authen, ILogger<NotificationJob> logger,
                    IHttpContextAccessor httpContextAccessor,
                    IMemoryCache cache)
        {
            _notificationService = notificationService;
            _bookingService = bookingService;
            _authen = authen;
            _httpContextAccessor = httpContextAccessor;
            _cache = cache;
            _logger = logger;
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
                var getBkOTWStsPending = await _bookingService.GetAllBookingByStsPendingCus();

                foreach (var booking in getBkOTWStsPending)
                {
                    var cacheKey = $"{booking.BarName}_{booking.BookingDate.Date}_{booking.BookingTime}_{booking.BookingId}";
                    if (_cache.TryGetValue(cacheKey, out _))
                    {
                        if (booking.BookingDate.Date == now.Date &&
                        booking.BookingTime + TimeSpan.FromHours(booking.TimeSlot) == roundedTimeOfDay &&
                        booking.TotalPrice != null &&
                        booking.Status == (int)PrefixValueEnum.PendingBooking)
                        {
                            await _bookingService.UpdateBookingStatus(booking.BookingId, (int)PrefixValueEnum.Completed, null);
                            _cache.Remove(cacheKey);
                            var messages = string.Format(PrefixKeyConstant.BOOKING_DRINKS_COMPLETED_NOTI, booking.BarName);
                            var creNotiRequest = new NotificationRequest
                            {
                                Title = booking.BarName,
                                Message = messages
                            };
                            await _notificationService.CreateNotificationAllCustomer(booking.AccountId, creNotiRequest);
                            _logger.LogInformation("Đã hoàn thành đơn hàng đặt với đồ uống");
                        }
                        else if (booking.BookingDate.Date == now.Date &&
                           booking.BookingTime + TimeSpan.FromHours(1) == roundedTimeOfDay &&
                           booking.TotalPrice == null &&
                           booking.Status == (int)PrefixValueEnum.PendingBooking)
                        {
                            await _bookingService.UpdateBookingStatus(booking.BookingId, (int)PrefixValueEnum.Cancelled, null);
                            var messages = string.Format(PrefixKeyConstant.BOOKING_CANCEL_NOTI, booking.BarName);
                            var creNotiRequest = new NotificationRequest
                            {
                                Title = booking.BarName,
                                Message = messages
                            };
                            await _notificationService.CreateNotificationAllCustomer(booking.AccountId, creNotiRequest);
                            _cache.Remove(cacheKey);
                            _logger.LogInformation("Đã hoàn thành đơn hàng đặt với đồ uống");
                        }
                    }
                }
                //if (getListBooking.IsNullOrEmpty())
                //{
                //    _logger.LogInformation("Không có danh sách booking nào đang chờ !");
                //}
                foreach (var booking in getListBooking)
                {
                    var cacheKey = $"{booking.BarName}_{booking.BookingDate.Date}_{booking.BookingTime}_{booking.BookingId}";
                    if (!_cache.TryGetValue(cacheKey, out _))
                    {
                        var messages = string.Format(PrefixKeyConstant.BOOKING_PENDING_NOTI, booking.BarName, booking.BookingDate.ToString("dd/MM/yyyy"));
                        var creNotiRequest = new NotificationRequest
                        {
                            Title = booking.BarName,
                            Message = messages
                        };
                        await _notificationService.CreateNotificationAllCustomer(booking.AccountId, creNotiRequest);
                        _cache.Set(cacheKey, true, TimeSpan.FromHours(2 + booking.TimeSlot));

                        _logger.LogInformation($"Đã gửi thông báo cho tài khoản {booking.AccountId} với {getListBooking.Count()} đơn đặt !");
                    }
                    else
                    {
                        if (booking.BookingDate.Date == now.Date &&
                           booking.BookingTime != roundedTimeOfDay &&
                           booking.Status == (int)PrefixValueEnum.PendingBooking)
                        {
                            continue;
                        }

                        var messages = string.Format(PrefixKeyConstant.BOOKING_REMIND_NOTI, booking.BarName, booking.BookingDate.ToString("dd/MM/yyyy"), roundedTimeOfDay);
                        var creNotiRequest = new NotificationRequest
                        {
                            Title = booking.BarName,
                            Message = messages
                        };
                        await _notificationService.CreateNotificationAllCustomer(booking.AccountId, creNotiRequest);
                        _logger.LogInformation($"Đã gửi thông báo cho tài khoản {booking.AccountId} với {getListBooking.Count()} đơn đặt !");
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
