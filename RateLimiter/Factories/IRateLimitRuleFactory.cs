using System;
using RateLimiter.Rules;
using RateLimiter.Stores;

namespace RateLimiter.Factories
{
    public interface IRateLimitRuleFactory
    {
        IRateLimitRule CreateRateLimitRule(RateLimitRuleTypes ruleType, RateLimitDataStoreTypes dataStoreType, int numberOfRequests, TimeSpan interval);
    }
}
