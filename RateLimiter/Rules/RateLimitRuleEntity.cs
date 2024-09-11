using System;

namespace RateLimiter.Rules;

public class RateLimitRuleEntity
{
    public int Id { get; init; }
    public required string ClientId { get; init; }
    public required string Resource { get; init; }
    public required RuleType RuleType { get; init; }
    public int MaxRequests { get; init; }
    public TimeSpan TimeSpan { get; init; }
    public TimeSpan RequiredTimeSpan { get; init; }
}