using RateLimiter.Rules;
using System.Collections.Generic;

namespace RateLimiter.Factory
{
    public interface IRateLimiterRuleFactory
    {
        List<IRateLimiterRule> CreateRules(string resource, string clientId);
    }
}
