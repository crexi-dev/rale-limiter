namespace RateLimiter.Abstractions;

public interface IRateLimiter
{
    public void LimitRequestsForToken(string token);
}