using System;
using RateLimiter.Abstractions;

namespace RateLimiter.Algorithms;

public record FixedWindowConfiguration : IRateLimitAlgorithmConfiguration
{
    public int MaxRequests { get; init; }

    public TimeSpan WindowDuration { get; init; }
}