using RateLimiter.Enums;

using System.Collections.Generic;

namespace RateLimiter.Config;

public class RateLimiterConfiguration
{
    public RateLimitingAlgorithm DefaultAlgorithm { get; set; }

    public int DefaultMaxRequests { get; set; }

    public int DefaultTimespanMilliseconds { get; set; }

    public List<RateLimiterRuleItemConfiguration> Rules { get; set; }

    public class RateLimiterRuleItemConfiguration
    {
        public string Name { get; set; }

        public LimiterType Type { get; set; }

        public LimiterDiscriminator Discriminator { get; set; }

        public string? CustomDiscriminatorType { get; set; }

        public string? DiscriminatorMatch { get; set; }

        public string? DiscriminatorRequestHeaderKey { get; set; }

        public int? MaxRequests { get; set; }

        public int? TimespanMilliseconds { get; set; }

        public RateLimitingAlgorithm? Algorithm { get; set; }
    }
}