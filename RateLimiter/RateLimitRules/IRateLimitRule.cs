using System;

namespace RateLimiterNS.RateLimitRules
{
    public interface IRateLimitRule
    {
        bool IsRequestAllowed(string token, DateTime requestTime);
    }
}