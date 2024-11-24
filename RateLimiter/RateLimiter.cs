using Crexi.API.Common.RateLimiter.Interfaces;
using Crexi.API.Common.RateLimiter.Models;
using System.Collections.Concurrent;

namespace Crexi.API.Common.RateLimiter
{
    public class RateLimiter : IRateLimiter
    {
        private readonly ConcurrentDictionary<string, IRateLimitRule> _resourceRules;

        public RateLimiter()
        {
            _resourceRules = new ConcurrentDictionary<string, IRateLimitRule>();
        }

        public void ConfigureResource(string resource, IRateLimitRule rule)
        {
            _resourceRules[resource] = rule;
        }

        public bool IsRequestAllowed(Client client, string resource)
        {
            if (_resourceRules.TryGetValue(resource, out var rule))
            {
                return rule.IsRequestAllowed(client, resource);
            }
            // Allow by default if no rules are configured
            return true;
        }
    }
}