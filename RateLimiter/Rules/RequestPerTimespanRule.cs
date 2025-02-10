using RateLimiter.Abstractions;
using RateLimiter.Enums;

using System;

namespace RateLimiter.Rules
{
    public class RequestPerTimespanRule : IDefineRateLimitRules
    {
        public LimiterType Type { get; } = LimiterType.RequestsPerTimespan;

        public string Name { get; set; }

        public LimiterDiscriminator Discriminator { get; set; }
        public string? DiscriminatorRequestHeaderKey { get; set; }

        public string? DiscriminatorMatch { get; set; }

        public RateLimitingAlgorithm Algorithm { get; set; } = RateLimitingAlgorithm.Default;

        public int MaxRequests { get; set; }

        public TimeSpan TimespanMilliseconds { get; set; }
    }
}
