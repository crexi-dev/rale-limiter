using RateLimiter.Abstractions;

using System;

namespace RateLimiter.Algorithms
{
    public class LeakyBucketConfiguration : IRateLimitAlgorithmConfiguration
    {
        public int Capacity { get; init; }

        public TimeSpan Interval { get; init; }
    }
}
