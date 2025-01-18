using RateLimiter.Interfaces;
using RateLimiter.Models;
using System.Collections.Generic;

namespace RateLimiter
{
    public class RateLimiter : IRateLimiter
    {
        private readonly Dictionary<string, RuleSet> _resourceRuleSet = new();

        public void Configure(string resource, RuleSet ruleSet)
        {
            _resourceRuleSet[resource] = ruleSet;
        }

        public bool IsAllowed(string clientId, string resource)
        {
            if (_resourceRuleSet.TryGetValue(resource, out var ruleSet))
            {
                return ruleSet.IsAllowed(clientId, resource);
            }

            // In case no rules are configured, the function is returning true.
            // But it also makes sense to return false or throw an exception since it's not registered in the system,
            // Or maybe use a default rule for not-registered cases.
            return true;
        }
    }
}
