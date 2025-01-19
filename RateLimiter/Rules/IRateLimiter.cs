using System;
namespace RateLimiter.Rules
{
    public interface IRateLimiter
    {
        bool IsAllowed(string clientToken, string uri);
    }
}
