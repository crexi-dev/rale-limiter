using RateLimiter.Interfaces;
using System;
using System.Collections.Generic;

namespace RateLimiter.RateLimiters
{
    public class ResourceRateLimiter
    {
        private readonly List<IRateLimitRule> _rules;

        public ResourceRateLimiter(List<IRateLimitRule> rules)
        {
            _rules = rules;
        }

        public bool IsRequestAllowed(string clientId)
        {
            foreach (var rule in _rules)
            {
                if (!rule.IsRequestAllowed(clientId))
                {
                    return false; // If any rule fails, deny the request
                }
            }
            return true; // Only allow if all rules pass
        }
    }

}
