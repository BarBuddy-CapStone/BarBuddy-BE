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
        void Add(string connectionId, string deviceToken);
        void Remove(string connectionId);
        string GetDeviceToken(string connectionId);
        List<string> GetConnectionIds(string deviceToken);
    }

    public class ConnectionMapping : IConnectionMapping
    {
        private readonly ConcurrentDictionary<string, string> _connections = new();

        public void Add(string connectionId, string deviceToken)
        {
            _connections.TryAdd(connectionId, deviceToken);
        }

        public void Remove(string connectionId)
        {
            _connections.TryRemove(connectionId, out _);
        }

        public string GetDeviceToken(string connectionId)
        {
            _connections.TryGetValue(connectionId, out var deviceToken);
            return deviceToken;
        }

        public List<string> GetConnectionIds(string deviceToken)
        {
            return _connections
                .Where(x => x.Value == deviceToken)
                .Select(x => x.Key)
                .ToList();
        }
    }
}
