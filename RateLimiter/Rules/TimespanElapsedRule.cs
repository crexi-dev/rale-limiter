using RateLimiter.Abstractions;
using RateLimiter.Enums;

using System;

namespace RateLimiter.Rules
{
    public class TimespanElapsedRule : IDefineARateLimitRule
    {
        public LimiterType Type { get; } = LimiterType.TimespanElapsed;

        public string Name { get; set; }

        public LimiterDiscriminator Discriminator { get; set; }

        public string? CustomDiscriminatorName { get; set; }

        public string? DiscriminatorRequestHeaderKey { get; set; }

        public string? DiscriminatorMatch { get; set; }

        public RateLimitingAlgorithm Algorithm { get; set; } = RateLimitingAlgorithm.Default;

        public TimeSpan TimespanSinceMilliseconds { get; set; }
    }
}
