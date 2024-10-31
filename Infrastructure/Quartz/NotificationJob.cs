using Application.DTOs.Notification;
using Application.Interfaces;
using Application.IService;
using Domain.Constants;
using Domain.CustomException;
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
                var getListBooking = await _bookingService.GetAllBookingByStsPending();
                if (getListBooking.IsNullOrEmpty())
                {
                    _logger.LogInformation("Không có danh sách booking nào đang chờ !");
                }
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
                        _cache.Set(cacheKey, true, TimeSpan.FromHours(3));

                        _logger.LogInformation($"Sent notification for account {booking.AccountId} with {getListBooking.Count()} bookings");
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
