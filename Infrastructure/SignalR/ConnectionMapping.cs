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
        void Add(string connectionId, Guid accountId);
        void Remove(string connectionId);
        IEnumerable<string> GetConnectionIds(Guid accountId);
        Guid? GetAccountId(string connectionId);
        IReadOnlyDictionary<string, Guid> GetAllConnections();
    }

    public class ConnectionMapping : IConnectionMapping
    {
        private readonly ConcurrentDictionary<string, Guid> _connections 
            = new ConcurrentDictionary<string, Guid>();

        public void Add(string connectionId, Guid accountId)
        {
            _connections.TryAdd(connectionId, accountId);
        }

        public void Remove(string connectionId)
        {
            _connections.TryRemove(connectionId, out _);
        }

        public IEnumerable<string> GetConnectionIds(Guid accountId)
        {
            return _connections
                .Where(x => x.Value == accountId)
                .Select(x => x.Key);
        }

        public Guid? GetAccountId(string connectionId)
        {
            return _connections.TryGetValue(connectionId, out var accountId) ? accountId : null;
        }

        public IReadOnlyDictionary<string, Guid> GetAllConnections()
        {
            return _connections;
        }
    }
}
