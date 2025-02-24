using System;
using RateLimiter.Constants;
using RateLimiter.Rules;

namespace RateLimiter.Factories
{
    public interface IRateLimitRuleFactory
    {
        IRateLimitRule CreateRule(
            RateLimitRuleTypes ruleType, 
            RateLimitDataStoreTypes dataStoreType, 
            DataStoreKeyTypes dataStoreKeyType,
            int numberOfRequests, 
            TimeSpan interval);
    }
}
