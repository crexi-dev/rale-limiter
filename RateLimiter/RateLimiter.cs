using RateLimiter.Interfaces;
using System.Collections.Generic;

namespace RateLimiter
{
    public class RateLimiter
    {
        private readonly Dictionary<string, List<IRateLimitingRule>> _rateLimitingRules = new Dictionary<string, List<IRateLimitingRule>>(); // TODO: Implement a proper storage mechanism

        public void AddRule(string resource, IRateLimitingRule rule)
        {
            if (!_rateLimitingRules.ContainsKey(resource))
            {
                _rateLimitingRules[resource] = new List<IRateLimitingRule>();
            }

            _rateLimitingRules[resource].Add(rule);
        }

        public bool IsRequestAllowed(string resource, string clientId)
        {
            if (!_rateLimitingRules.ContainsKey(resource))
            {
                return true; // By default, allow the request if no rules are defined for the resource
            }

            // Allow the request if all rules allow it
            return _rateLimitingRules[resource].TrueForAll(rule => rule.IsRequestAllowed(clientId));
        }
    }
}
