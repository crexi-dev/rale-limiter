using RateLimiter.Abstractions;
using RateLimiter.Enums;

using System;

namespace RateLimiter.Rules.Algorithms;

public class SlidingWindowRule : IRateLimitRuleAlgorithm
{
    public string Name { get; set; }

    public bool IsAllowed(string discriminator)
    {
        throw new NotImplementedException();
    }

    public RateLimitingAlgorithm Algorithm { get; set; } = RateLimitingAlgorithm.SlidingWindow;
}