using RateLimiter.Abstractions;
using RateLimiter.Enums;

using System;

namespace RateLimiter.Rules;

public class LeakyBucketRule : IRateLimitRuleAlgorithm
{
    public string Name { get; set; }

    public bool IsAllowed(string discriminator)
    {
        throw new NotImplementedException();
    }

    public LimiterDiscriminator Discriminator { get; set; }

    public RateLimitingAlgorithm Algorithm { get; set; } = RateLimitingAlgorithm.LeakyBucket;
}