using RateLimiter.Contracts;
using RateLimiter.Enums;

namespace RateLimiter.RateLimitSettings;

public class TimeSpanSinceLastCallRuleSettings : ISetting
{
    public int MinimumIntervalInMinutes { get; init; }
    
    public RegionType RegionType { get; init; }
}