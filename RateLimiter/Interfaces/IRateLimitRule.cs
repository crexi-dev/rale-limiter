using RateLimiter.Models;

namespace RateLimiter.Interfaces
{
    public interface IRateLimitRule
    {
        RateLimitResult IsRequestAllowed(string clientId);
    }
}