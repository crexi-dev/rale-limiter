using System;

namespace RateLimiter.Interfaces;

public interface IRateLimitingRule
{
    bool IsRequestAllowed(string accessToken, DateTime requestTime);
}
