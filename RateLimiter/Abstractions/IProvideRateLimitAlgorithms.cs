using RateLimiter.Enums;

using System;

namespace RateLimiter.Abstractions
{
    public interface IProvideRateLimitAlgorithms
    {
        IAmARateLimitAlgorithm GetAlgorithm(
            RateLimitingAlgorithm algo,
            int? maxRequests,
            TimeSpan? timespanMilliseconds);
    }
}
