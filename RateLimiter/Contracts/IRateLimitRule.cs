namespace RateLimiter.Contracts;

public interface IRateLimitRule
{
    bool IsRequestAllowed(string clientToken);
}