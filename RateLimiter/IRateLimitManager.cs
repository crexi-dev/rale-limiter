using RateLimiter.Attributes;

namespace RateLimiter;

public interface IRateLimitManager
{
    bool AddNewRequest(IRateLimit attribute, UserToken token);
}