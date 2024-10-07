using RateLimiter.Storages;
using System;
using System.Threading.Tasks;

namespace RateLimiter.Rules;
public class FixedDelayRule(TimeSpan delay) : IRateLimitRule
{
    public async Task<bool> IsRequestAllowedAsync(string clientId, string actionKey, IRateLimitStore store)
    {
        var currentTime = DateTime.UtcNow;
        var lastTime = await store.GetLastRequestTimeAsync(clientId, actionKey);

        if (lastTime == null || (currentTime - lastTime.Value) >= delay)
        {
            await store.SetLastRequestTimeAsync(clientId, actionKey, currentTime);
            return true;
        }
        else
        {
            return false;
        }
    }
}