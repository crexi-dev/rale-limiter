using RateLimiter.Abstractions;

using System;

namespace RateLimiter.Rules;

public class SlidingWindowRule : IRateLimitRule
{
    public bool IsAllowed(string discriminator)
    {
        throw new NotImplementedException();
    }
}