namespace RateLimiter.Abstractions;

public interface IRateLimitRule
{
    bool IsAllowed(string discriminator);
}