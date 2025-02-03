using System.Collections.Generic;

namespace RateLimiter.Rules
{
    public class RegionalRule : IRateLimitRule
    {
        private readonly Dictionary<string, IRateLimitRule> _regionRules;

        public RegionalRule(Dictionary<string, IRateLimitRule> regionRules)
        {
            _regionRules = regionRules;
        }

        public bool IsAllowed(string clientId, string resource)
        {
            var region = GetRegionFromClient(clientId);
            return _regionRules.TryGetValue(region, out var rule) && rule.IsAllowed(clientId, resource);
        }

        private string GetRegionFromClient(string clientId)
        {
            if (clientId.StartsWith("US-")) return "US";
            if (clientId.StartsWith("EU-")) return "EU";
            if (clientId.StartsWith("ASIA-")) return "ASIA";
            return "GLOBAL";
        }

        public void Cleanup()
        {
            foreach (var rule in _regionRules.Values)
            {
                rule.Cleanup();
            }
        }
    }
}
