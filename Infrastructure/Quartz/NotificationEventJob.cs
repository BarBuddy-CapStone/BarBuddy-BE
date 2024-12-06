using Application.DTOs.Fcm;
using Application.Interfaces;
using Application.IService;
using Domain.Constants;
using Domain.CustomException;
using Domain.Entities;
using Domain.Enums;
using Domain.IRepository;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Infrastructure.Quartz
{
    public class NotificationEventJob : IJob
    {
        private readonly IEventService _eventService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<NotificationEventJob> _logger;
        private readonly IFcmService _fcmService;

        public NotificationEventJob(
            IEventService eventService,
            IUnitOfWork unitOfWork,
            ILogger<NotificationEventJob> logger,
            IFcmService fcmService)
        {
            _eventService = eventService;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _fcmService = fcmService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var currentDate = DateTimeOffset.Now;
                var twoDaysLater = currentDate.AddDays(2);

                var events = await _unitOfWork.EventRepository.GetAsync(
                    filter: x => !x.IsDeleted && !x.IsHide,
                    includeProperties: "TimeEvent,Bar");

                foreach (var evt in events)
                {
                    if (evt.IsEveryWeek)
                    {
                        var weeklyEvents = evt.TimeEvent.Where(t => t.DayOfWeek.HasValue);
                        foreach (var timeEvent in weeklyEvents)
                        {
                            if ((int)twoDaysLater.DayOfWeek == timeEvent.DayOfWeek.Value)
                            {
                                await SendNotification(evt, timeEvent, twoDaysLater);
                            }
                        }
                    }
                    else
                    {
                        var oneTimeEvents = evt.TimeEvent.Where(t => t.Date.HasValue);
                        foreach (var timeEvent in oneTimeEvents)
                        {
                            if (timeEvent.Date.Value.Date == twoDaysLater.Date)
                            {
                                await SendNotification(evt, timeEvent, timeEvent.Date.Value);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý thông báo sự kiện");
                throw new CustomException.InternalServerErrorException("Lỗi hệ thống khi xử lý thông báo!");
            }
        }

        private async Task SendNotification(Event evt, TimeEvent timeEvent, DateTimeOffset eventDate)
        {
            await _fcmService.SendPushNotificationToAllDevices(
            new CreateNotificationRequest
            {
                BarId = evt.Bar.BarId,
                MobileDeepLink = $"com.fptu.barbuddy://event-detail/{evt.EventId}",
                WebDeepLink = $"event-detail/{evt.EventId}",
                ImageUrl = evt.Bar == null ? null : evt.Bar.Images.Split(',')[0],
                IsPublic = false,
                Message = string.Format(PrefixKeyConstant.EVENT_CONTENT_MESSAGE_NOTI,
                                        evt.EventName, evt?.Bar.BarName, eventDate.Date.ToString("dd/mm/yyyy"),
                                        timeEvent.StartTime,
                                        timeEvent.EndTime),
                Title = string.Format(PrefixKeyConstant.EVENT_TITLE_NOTI, evt?.Bar.BarName),
                Type = FcmNotificationType.EVENT
            });

            _logger.LogInformation($"Đã gửi thông báo cho sự kiện: {evt.EventName}");
        }
    }
}