using RateLimiter.Storages;
using System;
using System.Threading.Tasks;

namespace RateLimiter.Rules;
public class FixedWindowRule(int maxRequests, TimeSpan windowSize) : IRateLimitRule
{
    public async Task<bool> IsRequestAllowedAsync(string clientId, string actionKey, IRateLimitStore store)
    {
        var currentTime = DateTime.UtcNow;
        var windowStart = currentTime - windowSize;

        // Remove old request times
        await store.RemoveOldRequestTimesAsync(clientId, actionKey, windowStart);

        // Get current request times
        var requestTimes = await store.GetRequestTimesAsync(clientId, actionKey);

        if (requestTimes.Count < maxRequests)
        {
            await store.AddRequestTimeAsync(clientId, actionKey, currentTime);
            return true;
        }
        else
        {
            return false;
        }
    }
}