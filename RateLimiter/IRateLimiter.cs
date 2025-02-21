using RateLimiter.Rules;

namespace RateLimiter
{
    public interface IRateLimiter
    {
        void RegisterRule(string resourceId, IRateLimitRule rule);
        bool IsRequestAllowed(string resourceId, string userId);
    }
}
