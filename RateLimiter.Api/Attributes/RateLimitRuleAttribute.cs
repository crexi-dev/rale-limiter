using RateLimiter.Enums;

namespace RateLimiter.Api.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class RateLimitRuleAttribute(params RuleType[] ruleTypes) : Attribute
{
    public RuleType[] RuleTypes { get; } = ruleTypes;
}