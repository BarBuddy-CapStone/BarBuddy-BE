using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Linq;

namespace Infrastructure.SignalR
{
    public class NotificationHub : Hub
    {
        private readonly IConnectionMapping _connectionMapping;

        public NotificationHub(IConnectionMapping connectionMapping)
        {
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
    }
} 