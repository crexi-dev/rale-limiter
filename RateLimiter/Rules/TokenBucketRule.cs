using RateLimiter.Abstractions;

using System;

namespace RateLimiter.Rules;

public class TokenBucketRule : IRateLimitRule
{
    public bool IsAllowed(string discriminator)
    {
        throw new NotImplementedException();
    }
}