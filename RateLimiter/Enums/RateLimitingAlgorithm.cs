namespace RateLimiter.Enums;

public enum RateLimitingAlgorithm
{
    Default,
    FixedWindow,
    LeakyBucket,
    SlidingWindow,
    TimespanElapsed,
    TokenBucket
}