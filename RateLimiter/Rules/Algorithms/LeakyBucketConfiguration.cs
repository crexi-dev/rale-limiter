using System;

namespace RateLimiter.Rules.Algorithms
{
    public class LeakyBucketConfiguration
    {
        public int Capacity { get; init; }

        public TimeSpan Interval { get; init; }
    }
}
