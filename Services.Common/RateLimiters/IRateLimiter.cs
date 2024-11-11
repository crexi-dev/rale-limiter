using Services.Common.Models;

namespace Services.Common.RateLimiters;

public interface IRateLimiter
{
    bool IsRequestAllowed(RateLimitToken token);
}