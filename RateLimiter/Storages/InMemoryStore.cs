using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RateLimiter.Storages;
public class InMemoryStore : IRateLimitStore
{
    private readonly ConcurrentDictionary<(string clientId, string actionKey), List<DateTime>> requestTimes = new();
    private readonly ConcurrentDictionary<(string clientId, string actionKey), DateTime> lastRequestTime = new();

    public Task<List<DateTime>> GetRequestTimesAsync(string clientId, string actionKey)
    {
        var key = (clientId, actionKey);
        var times = requestTimes.GetOrAdd(key, _ => []);
        lock (times)
        {
            return Task.FromResult(times.ToList());
        }
    }

    public Task AddRequestTimeAsync(string clientId, string actionKey, DateTime timestamp)
    {
        var key = (clientId, actionKey);
        var times = requestTimes.GetOrAdd(key, _ => []);
        lock (times)
        {
            times.Add(timestamp);
        }
        return Task.CompletedTask;
    }

    public Task RemoveOldRequestTimesAsync(string clientId, string actionKey, DateTime windowStart)
    {
        var key = (clientId, actionKey);
        if (requestTimes.TryGetValue(key, out var times))
        {
            lock (times)
            {
                _=times.RemoveAll(t => t <= windowStart);
            }
        }
        return Task.CompletedTask;
    }

    public Task<DateTime?> GetLastRequestTimeAsync(string clientId, string actionKey)
    {
        var key = (clientId, actionKey);
        return lastRequestTime.TryGetValue(key, out var lastTime) ? Task.FromResult<DateTime?>(lastTime) : Task.FromResult<DateTime?>(null);
    }

    public Task SetLastRequestTimeAsync(string clientId, string actionKey, DateTime timestamp)
    {
        var key = (clientId, actionKey);
        lastRequestTime[key] = timestamp;
        return Task.CompletedTask;
    }
}
