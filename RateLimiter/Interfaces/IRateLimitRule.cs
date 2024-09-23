namespace RateLimiter.Interfaces
{
    public interface IRateLimitRule
    {
        bool IsRequestAllowed(string clientId);
    }

}
