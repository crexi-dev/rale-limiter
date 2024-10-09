using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace RateLimiter.Rules;

public sealed class TimeSinceLastCallRule(TimeSpan minimumTimeSpan) : IRateLimitRule
{
    // We might want to periodically clean up the dictionaries by removing tokens that haven't been used recently
    private readonly ConcurrentDictionary<string, DateTime> _requests = new();
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores = new();

    public async Task<bool> IsRequestAllowedAsync(string token)
    {
        var semaphore = _semaphores.GetOrAdd(token, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync();

        try
        {
            var currentTime = DateTime.UtcNow;
        
            var lastRequestTime = _requests.GetOrAdd(token, currentTime);

            var timeSinceLastRequest = currentTime - lastRequestTime;

            // The validation should be more sophisticated IRL 
            if (timeSinceLastRequest != TimeSpan.Zero // First request
                && timeSinceLastRequest < minimumTimeSpan)
                return false;
            
            _requests[token] = currentTime; // Update the timestamp
            return true;
        }
        finally
        {
            semaphore.Release();
        }
    }
    
    public TimeSpan GetTimeUntilReset(string token)
    {
        var currentTime = DateTime.UtcNow;

        if (!_requests.TryGetValue(token, out var lastRequestTime))
        {
            return TimeSpan.Zero; // No previous requests, so no reset time needed
        }

        var timeSinceLastRequest = currentTime - lastRequestTime;

        var timeUntilReset = minimumTimeSpan - timeSinceLastRequest;
        return timeUntilReset > TimeSpan.Zero ? timeUntilReset : TimeSpan.Zero;
    }    
}
