using System;

namespace RateLimiter.Rules
{
    public interface IRateLimitRuleBuilder
    {
        IRateLimitRuleBuilder Build();

        IRateLimitRuleBuilder WithTimeSinceLastCallRule(TimeSpan minInterval);

        IRateLimitRuleBuilder WithRequestCountRule(int maxRequests, TimeSpan timeSpan);

        IRateLimitRuleBuilder ApplyRule(bool condition, IRateLimitRule rule);

        bool IsAllowed();
    }
}