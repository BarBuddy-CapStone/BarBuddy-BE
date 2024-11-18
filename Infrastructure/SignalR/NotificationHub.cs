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
            var deviceToken = Context.GetHttpContext()?.Request.Query["deviceToken"].ToString();
            if (!string.IsNullOrEmpty(deviceToken))
            {
                _connectionMapping.Add(Context.ConnectionId, deviceToken);
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

        public async Task<int> GetUnreadCount(string deviceToken)
        {
            try
            {
                var unreadCount = await _fcmService.GetUnreadCount(deviceToken, null);
                
                // Gửi kết quả qua SignalR
                var connectionIds = _connectionMapping.GetConnectionIds(deviceToken);
                if (connectionIds.Any())
                {
                    await Clients.Clients(connectionIds)
                        .SendAsync("ReceiveUnreadCount", unreadCount);
                }
                
                return unreadCount;
            }
            catch (Exception ex)
            {
                // Log lỗi
                throw;
            }
        }
    }
} 