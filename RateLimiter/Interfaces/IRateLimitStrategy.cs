namespace RateLimiter.Interfaces
{
    internal interface IRateLimitStrategy
    {
        bool IsRequestAllowed(string clientToken);
    }
}
