using System.Collections.Generic;

namespace RateLimiter.Models
{
    public class RateLimitingRuleGroup
    {
        public string Name { get; set; } = default!;
        public List<RateLimitingConfigurationSet> ConfigurationSets { get; set; } = new();
    }
}
