using System.Collections.Generic;

namespace RateLimiter.Configuration
{
    public class RateLimiterConfig
    {
        public FixedWindowConfig FixedWindowConfig { get; set; } = new(3, 5);
        public CooldownConfig CooldownConfig { get; set; } = new(1);
        public IEnumerable<GeoBasedConfig> GeoBasedConfig { get; set; } = [];
        public IEnumerable<string> IpBlacklistConfig { get; set; } = [];
    }
}
