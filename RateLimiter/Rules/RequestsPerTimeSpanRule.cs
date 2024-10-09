using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RateLimiter.Rules;

public sealed class RequestsPerTimeSpanRule(int maxRequests, TimeSpan timeSpan) : IRateLimitRule
{
    // Ensure thread-safety by using ConcurrentDictionary for access
    private readonly ConcurrentDictionary<string, List<DateTime>> _requests = new();
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1); // Semaphore for controlling access

    public async Task<bool> IsRequestAllowedAsync(string token)
    {
        var currentTime = DateTime.UtcNow;
        
        var clientRequests = _requests.GetOrAdd(token, []);
        
        await _semaphore.WaitAsync(); // Acquire the lock
        
        try
        {
            // Remove requests that are older than the time span
            clientRequests.RemoveAll(x => (currentTime - x) > timeSpan);

            // Check if the number of requests is within the allowed limit
            if (clientRequests.Count >= maxRequests)
                return false;
            
            clientRequests.Add(currentTime); // Add the new request timestamp
            return true;
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    public TimeSpan GetTimeUntilReset(string token)
    {
        var currentTime = DateTime.UtcNow;
        var requests = _requests.GetOrAdd(token, []);

        if (requests.Count == 0)
        {
            return TimeSpan.Zero;
        }

        var oldestRequest = requests.First();
        var timeUntilReset = timeSpan - (currentTime - oldestRequest);
        
        return timeUntilReset > TimeSpan.Zero ? timeUntilReset : TimeSpan.Zero;
    }    
}
