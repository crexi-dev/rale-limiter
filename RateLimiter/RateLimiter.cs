using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using RateLimiter.Rules;

namespace RateLimiter
{
    public class RateLimiter
    {
        private readonly ConcurrentDictionary<string, List<IRateLimitRule>> _resourceRules = new();
        private readonly ILogger<RateLimiter> _logger;

        public RateLimiter(ILogger<RateLimiter> logger)
        {
            _logger = logger;
        }

        public void AddRule(string resource, IRateLimitRule rule)
        {
            _resourceRules.AddOrUpdate(resource, _ => new List<IRateLimitRule> { rule }, (_, rules) => { rules.Add(rule); return rules; });
        }

        public bool IsRequestAllowed(string clientId, string resource)
        {
            if (!_resourceRules.TryGetValue(resource, out var rules))
                return true;

            foreach (var rule in rules)
            {
                if (!rule.IsAllowed(clientId, resource))
                {
                    _logger.LogWarning($"Request blocked: Client {clientId}, Resource {resource}");
                    return false;
                }
            }
            return true;
        }

        public void Cleanup()
        {
            foreach (var rules in _resourceRules.Values)
            {
                foreach (var rule in rules)
                {
                    rule.Cleanup();
                }
            }
        }
    }
}
