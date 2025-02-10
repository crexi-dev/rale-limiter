using RateLimiter.Abstractions;

using System;
using RateLimiter.Enums;

namespace RateLimiter.Rules;

public class SlidingWindowRule : IRateLimitRuleAlgorithm
{
    public string Name { get; set; }

    public bool IsAllowed(string discriminator)
    {
        throw new NotImplementedException();
    }

    public LimiterDiscriminator Discriminator { get; set; }

    public RateLimitingAlgorithm Algorithm { get; set; } = RateLimitingAlgorithm.SlidingWindow;
}