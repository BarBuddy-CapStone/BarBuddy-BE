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
        private readonly IFcmService _fcmService;
        private readonly IConnectionMapping _connectionMapping;

        public NotificationHubService(
            IHubContext<NotificationHub> hubContext, 
            IFcmService fcmService,
            IConnectionMapping connectionMapping)
        {
            _hubContext = hubContext;
            _fcmService = fcmService;
            _connectionMapping = connectionMapping;
        }

        public async Task SendUnreadCount(string deviceToken, string accountId, int count)
        {
            if (!string.IsNullOrEmpty(accountId))
            {
                // Gửi cho user đã đăng nhập qua group
                await _hubContext.Clients.Group($"user_{accountId}")
                    .SendAsync("ReceiveUnreadCount", count);
            }
            else
            {
                // Gửi cho guest qua deviceToken
                var connectionIds = _connectionMapping.GetConnectionIds(deviceToken);
                if (connectionIds.Any())
                {
                    await _hubContext.Clients.Clients(connectionIds)
                        .SendAsync("ReceiveUnreadCount", count);
                }
            }
        }

        public async Task SendNotification(FcmNotification notification, bool isPublic)
        {
            var notificationData = new
            {
                id = notification.Id,
                title = notification.Title,
                message = notification.Message,
                type = notification.Type,
                timestamp = notification.CreatedAt,
                deepLink = notification.DeepLink,
                imageUrl = notification.ImageUrl,
                isRead = false,
                barId = notification.BarId,
                isPublic = isPublic,
                readAt = (DateTimeOffset?)null
            };

            // Lấy tất cả các device đang kết nối
            var connections = _connectionMapping.GetAllConnections();

            foreach (var connection in connections)
            {
                var deviceToken = connection.Value.DeviceToken;
                var accountId = connection.Value.AccountId;

                // Nếu là thông báo public
                if (isPublic)
                {
                    if (!string.IsNullOrEmpty(accountId))
                    {
                        // Gửi cho user đã đăng nhập qua group
                        await _hubContext.Clients.Group($"user_{accountId}")
                            .SendAsync("ReceiveNotification", notificationData);
                    }
                    else
                    {
                        // Gửi cho guest qua deviceToken
                        var connectionIds = _connectionMapping.GetConnectionIds(deviceToken);
                        if (connectionIds.Any())
                        {
                            await _hubContext.Clients.Clients(connectionIds)
                                .SendAsync("ReceiveNotification", notificationData);
                        }
                    }
                }
                // Nếu là thông báo private, chỉ gửi cho user đã đăng nhập
                else if (!string.IsNullOrEmpty(accountId))
                {
                    await _hubContext.Clients.Group($"user_{accountId}")
                        .SendAsync("ReceiveNotification", notificationData);
                }

                // Lấy số lượng thông báo chưa đọc mới
                var unreadCount = await _fcmService.GetUnreadNotificationCount(
                    deviceToken, 
                    !string.IsNullOrEmpty(accountId) ? Guid.Parse(accountId) : null);

                // Gửi update unread count
                await SendUnreadCount(deviceToken, accountId, unreadCount);
            }
        }
    }
}
