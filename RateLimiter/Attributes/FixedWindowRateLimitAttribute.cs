using RateLimiter.Rules;
using System;

namespace RateLimiter.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class FixedWindowRateLimitAttribute(int maxRequests, double windowSeconds) : RateLimitAttribute
{
    public int MaxRequests { get; } = maxRequests;
    public TimeSpan WindowSize { get; } = TimeSpan.FromSeconds(windowSeconds);

    public override IRateLimitRule CreateRule(IServiceProvider serviceProvider)
    {
        return new FixedWindowRule(MaxRequests, WindowSize);
    }
}