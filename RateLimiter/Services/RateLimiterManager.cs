using RateLimiter.Models;

namespace RateLimiter.Services
{
    public class RateLimiterManager
    {
        private readonly List<ClientRateLimitConfig> _clientRateLimits = new();

        public void AddRateLimitRules(List<ClientRateLimitConfig> clientConfigs)
        {
            foreach (var clientConfig in clientConfigs)
            {
                if (_clientRateLimits.Any(c => c.ClientId == clientConfig.ClientId))
                {
                    throw new ArgumentException($"Client ID '{clientConfig.ClientId}' is already registered.");
                }

                _clientRateLimits.Add(clientConfig);
            }
        }

        public RateLimitResult IsRequestAllowed(string clientId, string resource)
        {
            var result = new RateLimitResult { IsAllowed = true, RetryAfter = TimeSpan.Zero };

            var clientConfig = _clientRateLimits.FirstOrDefault(c => c.ClientId == clientId);
            if (clientConfig == null)
            {
                return new RateLimitResult { IsAllowed = false, RetryAfter = TimeSpan.Zero };
            }

            var resourceConfig = clientConfig.ResourceLimits.FirstOrDefault(r => r.Resource == resource);
            if (resourceConfig == null)
            {
                return new RateLimitResult { IsAllowed = false, RetryAfter = TimeSpan.Zero };
            }

            foreach (var rule in resourceConfig.Rules)
            {
                var ruleResult = rule.IsRequestAllowed(clientId);
                if (!ruleResult.IsAllowed && ruleResult.RetryAfter > result.RetryAfter)
                {
                    result = ruleResult; // Store the rule with the longest retry time
                }
            }

            return result;
        }
    }
}