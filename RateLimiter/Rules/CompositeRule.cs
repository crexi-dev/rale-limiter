using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RateLimiter.Rules;

public class CompositeRule(IEnumerable<IRateLimitRule> rules) : IRateLimitRule
{
    public async Task<bool> IsRequestAllowedAsync(string token)
    {
        foreach (var rule in rules)
        {
            if (!await rule.IsRequestAllowedAsync(token))
            {
                return false; // If any rule fails, the request is denied
            }
        }

        return true; // All rules passed
    }
    
    public TimeSpan GetTimeUntilReset(string token)
    {
        var maxTimeUntilReset = TimeSpan.Zero;

        foreach (var rule in rules)
        {
            var timeUntilReset = rule.GetTimeUntilReset(token);
            if (timeUntilReset > maxTimeUntilReset)
            {
                maxTimeUntilReset = timeUntilReset;
            }
        }

        return maxTimeUntilReset;
    }
}
