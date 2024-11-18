using Application.DTOs.Fcm;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.SignalR
{
    public class NotificationHubService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IFcmService _fcmService;

        public NotificationHubService(IHubContext<NotificationHub> hubContext, IFcmService fcmService)
        {
            _hubContext = hubContext;
            _fcmService = fcmService;
        }

        public async Task SendInAppNotification(string accountId, string title, string message)
        {
            var request = new CreateNotificationRequest
            {
                Title = title,
                Message = message,
                Type = FcmNotificationType.SYSTEM,
                IsPublic = false,
                Data = new Dictionary<string, string>
                {
                    { "accountId", accountId }
                }
            };

            await _fcmService.CreateAndSendNotification(request);
        }
    }
}
