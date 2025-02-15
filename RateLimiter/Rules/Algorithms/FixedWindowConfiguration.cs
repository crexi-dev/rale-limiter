using RateLimiter.Abstractions;

using System;

namespace RateLimiter.Rules.Algorithms;

public record FixedWindowConfiguration : IRateLimitAlgorithmConfiguration
{
    public int MaxRequests { get; init; }

    public TimeSpan WindowDuration { get; init; }
}