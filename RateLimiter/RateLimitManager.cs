using RateLimiter.Attributes;

namespace RateLimiter;

public class RateLimitManager : IRateLimitManager
{
    public bool IsRequestAllowed(IRateLimit attribute, string token)
    {
        return false;
    }
}

public interface IRateLimitManager
{
    bool IsRequestAllowed(IRateLimit attribute, string token);
}