using RateLimiter.Contracts;
using RateLimiter.Enums;

namespace RateLimiter.RateLimitSettings;

public class RatePerTimeSpanRuleSettings : ISetting
{
    public int RequestsCount { get; init; }
    
    public int IntervalInMinutes { get; init; }

    public RegionType RegionType { get; init; }
}