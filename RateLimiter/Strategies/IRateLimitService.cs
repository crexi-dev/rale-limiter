namespace RateLimiter.Strategies
{
    public interface IRateLimitService
    {
        public bool IsRequestAllowed();
    }
}