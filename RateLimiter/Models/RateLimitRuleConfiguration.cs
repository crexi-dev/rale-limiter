using RateLimiter.Enums;

namespace RateLimiter.Models;

// TODO: Break the configuration into separate classes for each type of rule and inject the rules into the middleware as generic implementations of IApplyARateLimit
public class RateLimitRuleConfiguration
{
    public int TooCloseToLastRequestRateLimitRuleMinimumTimeBetweenRequestsInSeconds { get; set; } = 10;

    public int TooManyInTimeSpanRateLimitRuleMaximumRequestsInTimeSpan { get; set; } = 2;

    public int TooManyInTimeSpanRateLimitRuleTimeSpanInSeconds { get; set; } = 10;

    public RateLimitRules Type { get; set; }
}