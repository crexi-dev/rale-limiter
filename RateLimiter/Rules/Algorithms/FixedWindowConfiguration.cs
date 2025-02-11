using System;

namespace RateLimiter.Rules.Algorithms;

public record FixedWindowConfiguration
{

    public int MaxRequests { get; init; }

    public TimeSpan WindowDuration { get; init; }
}