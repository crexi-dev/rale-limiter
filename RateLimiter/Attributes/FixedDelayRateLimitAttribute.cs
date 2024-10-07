using RateLimiter.Rules;
using System;

namespace RateLimiter.Attributes;
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class FixedDelayRateLimitAttribute(double delaySeconds) : RateLimitAttribute
{
    public TimeSpan Delay { get; } = TimeSpan.FromSeconds(delaySeconds);

    public override IRateLimitRule CreateRule(IServiceProvider serviceProvider)
    {
        return new FixedDelayRule(Delay);
    }
}