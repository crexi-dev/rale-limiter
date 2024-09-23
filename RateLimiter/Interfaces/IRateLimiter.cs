using System;

namespace RateLimiter.Interfaces
{
    [Obsolete("used for static rule.")]
    public interface IRateLimiter
    {
        bool IsRequestAllowed(string clientId);
    }
}
