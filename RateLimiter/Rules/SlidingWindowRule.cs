using RateLimiter.Abstractions;

using System;
using RateLimiter.Config;

namespace RateLimiter.Rules;

public class SlidingWindowRule : IRateLimitRule
{
    public bool IsAllowed(string discriminator)
    {
        throw new NotImplementedException();
    }

    public LimiterDiscriminator Discriminator { get; set; }
}