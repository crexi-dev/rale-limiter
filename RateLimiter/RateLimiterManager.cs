using RateLimiter.Data;
using RateLimiter.Factory;
using RateLimiter.Rules;
using System;
using System.Collections.Generic;

namespace RateLimiter
{
    /// <summary>
    /// Manages multiple rate-limiting rules and applies them to a request based on the API resource and clientId.
    /// </summary>
    public class RateLimiterManager
    {
        private readonly IRateLimiterRuleFactory _rateLimitRuleFactory;
        private readonly IRateLimiterDataStore _store;

        public RateLimiterManager(IRateLimiterRuleFactory rateLimitRuleFactory, IRateLimiterDataStore store)
        {
            _rateLimitRuleFactory = rateLimitRuleFactory;
            _store = store;
        }

        public RateLimiterResult CheckRequest(string clientId, string resource)
        {
            // Get the rules for the specific resource and clientId from the factory
            List<IRateLimiterRule> rules = _rateLimitRuleFactory.CreateRules(resource, clientId);

            // Apply all the rules to the request
            foreach (var rule in rules)
            {
                var result = rule.CheckLimit(clientId, resource);
                if (!result.IsAllowed)
                    return result;
            }

            // Update time and count
            var clientData = _store.GetClientData(clientId, resource);
            clientData.StartTime = DateTime.UtcNow;
            _store.IncrementRequestCount(clientId, resource);

            // Allowed
            return RateLimiterResult.Allowed();
        }
    }
}
