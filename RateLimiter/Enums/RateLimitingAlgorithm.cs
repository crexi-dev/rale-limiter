namespace RateLimiter.Enums;

public enum RateLimitingAlgorithm
{
    Default,
    TokenBucket,
    LeakyBucket,
    FixedWindow,
    SlidingWindow
}