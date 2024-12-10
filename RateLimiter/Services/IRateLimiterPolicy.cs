namespace RateLimiter.Services
{
    public interface IRateLimiterPolicy
    {
        public bool IsApplicable(string apiKey, DateTime requestTime);
    }
}
