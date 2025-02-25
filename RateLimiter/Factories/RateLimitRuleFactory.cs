using System;
using RateLimiter.Constants;
using RateLimiter.Exceptions;
using RateLimiter.Rules;
using RateLimiter.Stores;

namespace RateLimiter.Factories
{
    public class RateLimitRuleFactory : IRateLimitRuleFactory
    {
        private readonly IRateLimitDataStoreFactory _rateLimitDataStoreFactory;

        public RateLimitRuleFactory(IRateLimitDataStoreFactory rateLimitDataStoreFactory)
        {
            _rateLimitDataStoreFactory = rateLimitDataStoreFactory;
        }

        public IRateLimitRule CreateRule(
            RateLimitRuleTypes ruleType, 
            RateLimitDataStoreTypes dataStoreType, 
            DataStoreKeyTypes dataStoreKeyType,
            int numberOfRequests, 
            TimeSpan interval)
        {
            var dataStore = _rateLimitDataStoreFactory.CreateDataStore(dataStoreType);
            var keyGenerator = new DataStoreKeyGenerator(dataStoreKeyType);

            switch (ruleType)
            {
                case RateLimitRuleTypes.RequestsPerTimeSpan:
                    return new RequestsPerTimeSpanRule(numberOfRequests, interval, dataStore, keyGenerator);
                default:
                    throw new RuleTypeNotImplementedException(ruleType);
            }
        }
    }
}
