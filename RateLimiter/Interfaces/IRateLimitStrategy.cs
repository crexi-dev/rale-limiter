namespace RateLimiter.Interfaces
{
    public interface IRateLimitStrategy
    {
        bool IsRequestAllowed(string clientToken);
    }
}
