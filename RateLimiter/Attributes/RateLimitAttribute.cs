using RateLimiter.Rules;
using System;

namespace RateLimiter.Attributes;
public abstract class RateLimitAttribute : Attribute
{
    public abstract IRateLimitRule CreateRule(IServiceProvider serviceProvider);
}
