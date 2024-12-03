using Application.DTOs.Fcm;
using Application.Interfaces;
using Domain.Entities;
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
        private readonly IConnectionMapping _connectionMapping;

        public NotificationHubService(
            IHubContext<NotificationHub> hubContext,
            IConnectionMapping connectionMapping)
        {
            _hubContext = hubContext;
            _connectionMapping = connectionMapping;
        }

        public async Task SendNotification(FcmNotification notification, Guid accountId)
        {
            var notificationData = new
            {
                id = notification.Id,
                title = notification.Title,
                message = notification.Message,
                type = notification.Type,
                timestamp = notification.CreatedAt,
                mobileDeepLink = notification.MobileDeepLink,
                webDeepLink = notification.WebDeepLink,
                imageUrl = notification.ImageUrl,
                isRead = false,
                barId = notification.BarId,
                isPublic = notification.IsPublic,
                readAt = (DateTimeOffset?)null
            };

            await _hubContext.Clients.Group($"user_{accountId}")
                .SendAsync("ReceiveNotification", notificationData);
        }

        public async Task SendUnreadCount(Guid accountId, int count)
        {
            await _hubContext.Clients.Group($"user_{accountId}")
                .SendAsync("ReceiveUnreadCount", count);
        }
    }
}
