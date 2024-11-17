using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Linq;
using Domain.IRepository;

namespace Infrastructure.SignalR
{
    public class NotificationHub : Hub
    {
        private readonly IConnectionMapping _connectionMapping;
        private readonly IUnitOfWork _unitOfWork;

        public NotificationHub(IConnectionMapping connectionMapping, IUnitOfWork unitOfWork)
        {
            _connectionMapping = connectionMapping;
            _unitOfWork = unitOfWork;
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

        public async Task GetUnreadCount(string deviceToken, Guid? accountId = null)
        {
            int unreadCount = 0;
            
            if (accountId.HasValue)
            {
                unreadCount = (await _unitOfWork.FcmNotificationCustomerRepository
                        .GetAsync(nc => nc.CustomerId == accountId && nc.DeviceToken == deviceToken && !nc.IsRead)).Count();
            }
            else if (!string.IsNullOrEmpty(deviceToken))
            {
                var notifications = await _unitOfWork.FcmNotificationCustomerRepository
                    .GetAsync(nc => nc.DeviceToken == deviceToken && !nc.IsRead);
                unreadCount = notifications.Count();
            }

            var connectionIds = _connectionMapping.GetConnectionIds(deviceToken);
            if (connectionIds.Any())
            {
                await Clients.Clients(connectionIds).SendAsync("ReceiveUnreadCount", unreadCount);
            }
        }
    }
} 