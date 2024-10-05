using RateLimiter.Data;
using RateLimiter.Rules;
using System;
using System.Collections.Generic;

namespace RateLimiter.Factory
{
    public class RateLimiterRuleFactory : IRateLimiterRuleFactory
    {
        private readonly IRateLimiterDataStore _store;

        public RateLimiterRuleFactory(IRateLimiterDataStore store)
        {
            _store = store;
        }

        /// <summary>
        /// TODO: The rules can be stored in the datastore for each clients and resources. Add rules which are stored in the datastore.
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public List<IRateLimiterRule> CreateRules(string resource, string clientId)
        {
            List<IRateLimiterRule> rules = new List<IRateLimiterRule>();

            // TODO: Get customer info from another data store like Redis or DB and add rules based on client-id and resources.
            // Define rules based on specific ClientId
            if (clientId == "premium-client")
            {
                // Premium clients may have more than regular customer request limit
                rules.Add(new RequestCount(500, TimeSpan.FromHours(1), _store));
            }
            if (clientId == "regular-client")
            {
                // Regular clients may have a higher request limit
                rules.Add(new RequestCount(200, TimeSpan.FromHours(1), _store));
            }
            else if (clientId == "basic-client")
            {
                // Basic clients may have a stricter rate limit and time limit between requests
                rules.Add(new RequestCount(100, TimeSpan.FromHours(1), _store));
            }
            else if (clientId == "free-client")
            {
                // Default rules for free clients
                rules.Add(new RequestCount(50, TimeSpan.FromHours(1), _store));
                rules.Add(new TimeLimitBetweenRequests(TimeSpan.FromSeconds(5), _store));
            }

            // Example: Additional rules based on resource
            if (resource == "api/resource1")
            {
                rules.Add(new TimeLimitBetweenRequests(TimeSpan.FromSeconds(5), _store));
            }

            return rules;
        }
    }
}
