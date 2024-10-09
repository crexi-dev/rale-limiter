using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RateLimiter;

public interface IRateLimiter
{
    Task<(bool isAllowed, DateTime nextAllowedTime)> IsRequestAllowedAsync(string resourceName, string token);
}

public class RateLimiter : IRateLimiter
{
    private readonly Dictionary<string, IRateLimitRule> _rateLimitRules = new();

    // Assign rule to a resource
    public void ConfigureRateLimitRule(string resourceName, IRateLimitRule rule)
    {
        _rateLimitRules[resourceName] = rule;
    }

    // Check if the request is allowed
    public async Task<(bool isAllowed, DateTime nextAllowedTime)> IsRequestAllowedAsync(string resourceName, string token)
    {
        if (!_rateLimitRules.TryGetValue(resourceName, out var rule))
        {
            throw new InvalidOperationException("No rate limiting rule configured for this resource.");
        }

        var isAllowed = await rule.IsRequestAllowedAsync(token);
        
        var nextAllowedTime = DateTime.UtcNow;
        if (!isAllowed)
        {
            nextAllowedTime = DateTime.UtcNow.Add(rule.GetTimeUntilReset(token));
        }
        
        return (isAllowed, nextAllowedTime);
    }
}
