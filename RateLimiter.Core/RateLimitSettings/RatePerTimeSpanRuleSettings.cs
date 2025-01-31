using RateLimiter.Contracts;

namespace RateLimiter.RateLimitSettings;

public class RatePerTimeSpanRuleSettings : ISetting
{
    public int RequestsCount { get; init; }
    
    public int IntervalInMinutes { get; init; }

    public string Region { get; init; } = string.Empty;
}