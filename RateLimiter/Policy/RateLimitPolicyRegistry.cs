using System;
using System.Collections.Generic;

namespace RateLimiter.Policy
{
    public class RateLimitPolicyRegistry()
    {
        private readonly Dictionary<string, RateLimitPolicy> _policies = [];

        public void AddPolicy(string name, Action<RateLimitPolicy> configure)
        {
            var policy = new RateLimitPolicy();
            configure(policy);
            // overrides if already exists
            _policies[name] = policy;
        }

        public RateLimitPolicy? GetPolicy(string name)
        {
            return _policies.TryGetValue(name, out var policy) ? policy : null;
        }
    }
}
