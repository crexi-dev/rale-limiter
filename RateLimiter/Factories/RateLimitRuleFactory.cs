using System;
using RateLimiter.Rules;
using RateLimiter.Stores;

namespace RateLimiter.Factories
{
    public class RateLimitRuleFactory : IRateLimitRuleFactory
    {
        private static readonly string UnknownRuleError = "Unknown RateLimitRuleType: {0}";
        private readonly IRateLimitDataStoreFactory _rateLimitDataStoreFactory;

        public RateLimitRuleFactory(IRateLimitDataStoreFactory rateLimitDataStoreFactory)
        {
            _rateLimitDataStoreFactory = rateLimitDataStoreFactory;
        }

        public IRateLimitRule CreateRateLimitRule(RateLimitRuleTypes ruleType, RateLimitDataStoreTypes dataStoreTypeint, int numberOfRequests, TimeSpan interval)
        {
            var dataStore = _rateLimitDataStoreFactory.CreateDataStore(dataStoreTypeint);

            switch (ruleType)
            {
                case RateLimitRuleTypes.RequestsPerTimeSpan:
                    return new RequestsPerTimeSpanRule(numberOfRequests, interval, dataStore);
                case RateLimitRuleTypes.RequestsPerUserPerTimeSpan:
                    return new RequestsPerUserPerTimeSpanRule(numberOfRequests, interval, dataStore);
                default:
                    var errorMessage = string.Format(UnknownRuleError, ruleType.ToString());
                    throw new NotImplementedException(errorMessage);
            }
        }
    }
}
