using Crexi.API.Common.RateLimiter.Interfaces;
using Crexi.API.Common.RateLimiter.Models;
using System.Collections.Generic;

namespace Crexi.API.Common.RateLimiter.Rules
{
    public class CompositeRateLimitRule : IRateLimitRule
    {
        private readonly IEnumerable<IRateLimitRule> _rules;

        public CompositeRateLimitRule(IEnumerable<IRateLimitRule> rules)
        {
            _rules = rules;
        }

        public bool IsRequestAllowed(Client client, string resource)
        {
            foreach (var rule in _rules)
            {
                if (!rule.IsRequestAllowed(client, resource))
                {
                    return false;
                }
            }
            return true;
        }
    }

}