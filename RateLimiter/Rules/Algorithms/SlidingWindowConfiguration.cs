using System;

namespace RateLimiter.Rules.Algorithms
{
    public class SlidingWindowConfiguration
    {
        public int MaxRequests { get; init; }

        public TimeSpan WindowDuration { get; init; }
    }
}
