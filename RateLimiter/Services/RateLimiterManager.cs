using RateLimiter.Models;

namespace RateLimiter.Services
{
    public class RateLimiterManager
    {
        private readonly IReadOnlyList<ClientRateLimitConfig> _clientRateLimits;

        public RateLimiterManager(IEnumerable<ClientRateLimitConfig> clientConfigs)
        {
            // Validate for duplicate client IDs
            var duplicateClients = clientConfigs
                .GroupBy(c => c.ClientId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateClients.Any())
            {
                throw new ArgumentException(
                    $"Duplicate client IDs found: {string.Join(", ", duplicateClients)}");
            }

            _clientRateLimits = clientConfigs.ToList();
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