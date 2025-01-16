using System;

namespace RateLimiter.Domain;

public class RateLimitResult
{
    public bool IsRateLimited { get; }
    public int RemainingRequests { get; }
    public TimeSpan RetryAfter { get; }

    public RateLimitResult(
        bool isRateLimited,
        int remainingRequests,
        TimeSpan retryAfter)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(remainingRequests, -1, nameof(remainingRequests));
        ArgumentOutOfRangeException.ThrowIfLessThan(retryAfter, TimeSpan.Zero, nameof(retryAfter));

        IsRateLimited = isRateLimited;
        RemainingRequests = remainingRequests;
        RetryAfter = retryAfter;
    }
}
