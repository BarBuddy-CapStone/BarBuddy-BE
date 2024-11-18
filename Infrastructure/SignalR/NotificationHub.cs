using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Linq;
using Domain.IRepository;
using Infrastructure.Integrations;
using Application.Interfaces;

namespace Infrastructure.SignalR
{
    public class NotificationHub : Hub
    {
        private readonly IFcmService _fcmService;
        private readonly IConnectionMapping _connectionMapping;

        public NotificationHub(IFcmService fcmService, IConnectionMapping connectionMapping)
        {
            _fcmService = fcmService;
            _connectionMapping = connectionMapping;
        }

        public override async Task OnConnectedAsync()
        {
            var deviceToken = Context.GetHttpContext().Request.Query["deviceToken"].ToString();
            var accountId = Context.GetHttpContext().Request.Query["accountId"].ToString();

            if (!string.IsNullOrEmpty(deviceToken))
            {
                _connectionMapping.Add(deviceToken, Context.ConnectionId);
                
                if (!string.IsNullOrEmpty(accountId))
                {
                    // Thêm connection vào group của user
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{accountId}");
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

        public async Task GetUnreadCount(string deviceToken, Guid? accountId = null)
        {
            try
            {
                var unreadCount = await _fcmService.GetUnreadCount(deviceToken, accountId);
                
                if (accountId.HasValue)
                {
                    // Gửi cho user đã đăng nhập qua group
                    await Clients.Group($"user_{accountId}")
                        .SendAsync("ReceiveUnreadCount", unreadCount);
                }
                else
                {
                    // Gửi cho guest user qua connectionId
                    var connectionIds = _connectionMapping.GetConnectionIds(deviceToken);
                    if (connectionIds.Any())
                    {
                        await Clients.Clients(connectionIds)
                            .SendAsync("ReceiveUnreadCount", unreadCount);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log lỗi
                throw;
            }
        }
    }
} 