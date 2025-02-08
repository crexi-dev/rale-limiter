using RateLimiter.Abstractions;

using System.Collections.Generic;

namespace RateLimiter;

public class RateLimiterRulesFactory : IProvideRateLimitRules
{
    public IEnumerable<IRateLimitRule> GetRules()
    {
        // Load built-in rules

        // Load rules defined via appSettings
        return new List<IRateLimitRule>();
    }
}