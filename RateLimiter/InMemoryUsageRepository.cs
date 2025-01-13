// In the RateLimiter project, create an InMemoryUsageRepository.cs
using System.Collections.Concurrent;
using RateLimiter.Interfaces;

namespace RateLimiter
{
    public class InMemoryUsageRepository : IUsageRepository
    {
        private readonly ConcurrentDictionary<string, RequestUsage> _store = new ConcurrentDictionary<string, RequestUsage>();

        RequestUsage IUsageRepository.GetUsageForClient(string clientToken)
        {
            _store.TryGetValue(clientToken, out var usage);
            
            return usage ?? new RequestUsage();
        }

        void IUsageRepository.UpdateUsageForClient(string clientToken, RequestUsage usage)
        {
            _store[clientToken] = usage;
        }
    }
}
