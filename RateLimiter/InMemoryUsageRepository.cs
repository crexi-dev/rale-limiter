using System.Collections.Concurrent;
using RateLimiter.Interfaces;

namespace RateLimiter
{
    public class InMemoryUsageRepository : IUsageRepository
    {
        private readonly ConcurrentDictionary<string, RequestUsage> _store = new ConcurrentDictionary<string, RequestUsage>();

        public RequestUsage GetUsageForClient(string clientToken)
        {
            _store.TryGetValue(clientToken, out var usage);
            
            return usage ?? new RequestUsage();
        }

        public void UpdateUsageForClient(string clientToken, RequestUsage usage)
        {
            _store[clientToken] = usage;
        }
    }
}
