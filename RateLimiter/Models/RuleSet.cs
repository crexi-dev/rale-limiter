using System.Collections.Generic;
using RateLimiter.Interfaces;

namespace RateLimiter.Models
{
    public class RuleSet
    {
        private readonly List<IRule> _rules = new();

        public void AddRule(IRule rule)
        {
            _rules.Add(rule);
        }

        public bool IsAllowed(string clientId, string resource)
        {
            foreach (var rule in _rules)
            {
                if (!rule.IsAllowed(clientId, resource))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
