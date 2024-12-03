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

        public NotificationHub(IConnectionMapping connectionMapping)
        {
            _connectionMapping = connectionMapping;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            if (httpContext != null)
            {
                var accountIdStr = httpContext.Request.Query["accountId"].ToString();
                
                if (string.IsNullOrEmpty(accountIdStr))
                {
                    throw new HubException("AccountId is required");
                }

                var accountId = Guid.Parse(accountIdStr);
                _connectionMapping.Add(Context.ConnectionId, accountId);
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{accountId}");
                Console.WriteLine($"User {accountId} connected");
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _connectionMapping.Remove(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task RequestUnreadCount()
        {
            var accountId = _connectionMapping.GetAccountId(Context.ConnectionId);
            if (accountId.HasValue)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{accountId}");
            }
        }
    }
} 