using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.SignalR
{
    public interface IConnectionMapping
    {
        void Add(string connectionId, string deviceToken, string accountId = null);
        void Remove(string connectionId);
        IEnumerable<string> GetConnectionIds(string deviceToken);
        string GetDeviceToken(string connectionId);
        string GetAccountId(string connectionId);
        IReadOnlyDictionary<string, (string DeviceToken, string AccountId)> GetAllConnections();
    }

    public class ConnectionMapping : IConnectionMapping
    {
        private readonly ConcurrentDictionary<string, (string DeviceToken, string AccountId)> _connections 
            = new ConcurrentDictionary<string, (string DeviceToken, string AccountId)>();

        public void Add(string connectionId, string deviceToken, string accountId = null)
        {
            _connections.TryAdd(connectionId, (deviceToken, accountId));
        }

        public void Remove(string connectionId)
        {
            _connections.TryRemove(connectionId, out _);
        }

        public IEnumerable<string> GetConnectionIds(string deviceToken)
        {
            return _connections
                .Where(x => x.Value.DeviceToken == deviceToken)
                .Select(x => x.Key);
        }

        public string GetDeviceToken(string connectionId)
        {
            return _connections.TryGetValue(connectionId, out var value) ? value.DeviceToken : null;
        }

        public string GetAccountId(string connectionId)
        {
            return _connections.TryGetValue(connectionId, out var value) ? value.AccountId : null;
        }

        public IReadOnlyDictionary<string, (string DeviceToken, string AccountId)> GetAllConnections()
        {
            return _connections;
        }
    }
}
