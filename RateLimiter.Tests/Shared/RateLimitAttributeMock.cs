using RateLimiter.Attributes;
using RateLimiter.Rules;
using System;

namespace RateLimiter.Tests.Shared;

public class RateLimitAttributeMock : RateLimitAttribute
{
    public override IRateLimitRule CreateRule(IServiceProvider serviceProvider)
    {
        return (IRateLimitRule)serviceProvider.GetService(typeof(IRateLimitRule));
    }
}
