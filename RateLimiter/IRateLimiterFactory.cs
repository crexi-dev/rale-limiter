using RateLimiter.Model;
using RateLimiter.Strategies;

namespace RateLimiter
{
    public interface IRateLimiterFactory
    {
        IRateLimitService CreateRateLimiter(string resourceUrl, ClientModel clientData);
    }
}