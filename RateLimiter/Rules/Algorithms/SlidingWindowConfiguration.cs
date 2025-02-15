using RateLimiter.Abstractions;

using System;

namespace RateLimiter.Rules.Algorithms
{
    public class SlidingWindowConfiguration : IRateLimitAlgorithmConfiguration
    {
        public int MaxRequests { get; init; }

        public TimeSpan WindowDuration { get; init; }
    }
}
