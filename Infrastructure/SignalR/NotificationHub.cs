using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Linq;
using Application.Interfaces;

namespace Infrastructure.SignalR
{
    public class NotificationHub : Hub
    {
        private readonly IConnectionMapping _connectionMapping;
        private readonly IFcmService _fcmService;

        public NotificationHub(
            IConnectionMapping connectionMapping,
            IFcmService fcmService)
        {
            _connectionMapping = connectionMapping;
            _fcmService = fcmService;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            if (httpContext != null)
            {
                var deviceToken = httpContext.Request.Query["deviceToken"].ToString();
                var accountId = httpContext.Request.Query["accountId"].ToString();

                if (string.IsNullOrEmpty(deviceToken))
                {
                    throw new HubException("DeviceToken is required");
                }

                if (!string.IsNullOrEmpty(accountId))
                {
                    _connectionMapping.Add(Context.ConnectionId, deviceToken, accountId);
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{accountId}");
                    Console.WriteLine($"User {accountId} connected with device {deviceToken}");
                }
                else
                {
                    _connectionMapping.Add(Context.ConnectionId, deviceToken);
                    Console.WriteLine($"Guest connected with device {deviceToken}");
                }
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _connectionMapping.Remove(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendNotification(string deviceToken, object notification)
        {
            var connectionIds = _connectionMapping.GetConnectionIds(deviceToken);
            if (connectionIds.Any())
            {
                await Clients.Clients(connectionIds).SendAsync("ReceiveNotification", notification);
            }
        }

        public async Task SendBroadcast(string message)
        {
            await Clients.All.SendAsync("ReceiveBroadcast", message);
        }

        public async Task RequestUnreadCount()
        {
            var deviceToken = _connectionMapping.GetDeviceToken(Context.ConnectionId);
            var accountId = _connectionMapping.GetAccountId(Context.ConnectionId);

            if (!string.IsNullOrEmpty(deviceToken))
            {
                var unreadCount = await _fcmService.GetUnreadNotificationCount(
                    deviceToken,
                    !string.IsNullOrEmpty(accountId) ? Guid.Parse(accountId) : null);

                if (!string.IsNullOrEmpty(accountId))
                {
                    await Clients.Group($"user_{accountId}")
                        .SendAsync("ReceiveUnreadCount", unreadCount);
                }
                else
                {
                    await Clients.Caller.SendAsync("ReceiveUnreadCount", unreadCount);
                }
            }
        }
    }
} 