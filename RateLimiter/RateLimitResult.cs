using System;

namespace RateLimiter;

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
        IsRateLimited = isRateLimited;
        RemainingRequests = remainingRequests;
        RetryAfter = retryAfter;
    }
}
