using RateLimiter.Model;

namespace RateLimiter.Interface
{
    public interface IRateLimiterService
    {
        bool Validate(Request request);
    }
}
