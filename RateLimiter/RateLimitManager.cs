using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter
{
    public class RateLimitManager
    {
        private readonly Dictionary<string, List<IRateLimitRule>> _resourceRules = new();

        public void AddRule(string resource, IRateLimitRule rule)
        {
            if (!_resourceRules.ContainsKey(resource))
            {
                _resourceRules[resource] = new List<IRateLimitRule>();
            }

            _resourceRules[resource].Add(rule);
        }

        public bool IsRequestAllowed(RateLimitContext context)
        {
            if (!_resourceRules.ContainsKey(context.Resource))
            {
                return true; // No rules applied, allow by default
            }

            var rules = _resourceRules[context.Resource];
            return rules.All(rule => rule.IsRequestAllowed(context));
        }
    }

}
