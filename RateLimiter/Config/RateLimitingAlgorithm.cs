namespace RateLimiter.Config;

public enum RateLimitingAlgorithm
{
    TokenBucket,
    LeakyBucket,
    FixedWindow,
    SlidingWindow
}