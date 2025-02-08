using RateLimiter.Abstractions;

using System.Collections.Generic;

namespace RateLimiter;

public class RateLimiterRulesFactory : IProvideRateLimitRules
{
    public IEnumerable<IRateLimitRule> GetRules()
    {
        return new List<IRateLimitRule>();
    }
}