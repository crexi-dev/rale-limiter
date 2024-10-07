using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RateLimiter.Storages;
public interface IRateLimitStore
{
    Task<List<DateTime>> GetRequestTimesAsync(string clientId, string actionKey);
    Task AddRequestTimeAsync(string clientId, string actionKey, DateTime timestamp);
    Task RemoveOldRequestTimesAsync(string clientId, string actionKey, DateTime windowStart);
    Task<DateTime?> GetLastRequestTimeAsync(string clientId, string actionKey);
    Task SetLastRequestTimeAsync(string clientId, string actionKey, DateTime timestamp);
}
