namespace RateLimiter.Enums;

public enum RateLimitingAlgorithm
{
    Default,
    FixedWindow,
    LeakyBucket,
    SlidingWindow,
    TokenBucket
}