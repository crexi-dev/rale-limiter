using RateLimiter.Abstractions;

using System;

namespace RateLimiter.Algorithms
{
    public class SlidingWindowConfiguration : IRateLimitAlgorithmConfiguration
    {
        public int MaxRequests { get; init; }

        public TimeSpan WindowDuration { get; init; }
    }
}
