using System;

namespace RateLimiter;

public class RuleState
{
    private const StringComparison ScopeComparison = StringComparison.InvariantCultureIgnoreCase;

    public int RequestCount { get; }
    public DateTime? LastRequestTime { get; set; }

    public RuleState(int requestCount, DateTime? lastRequestTime = null)
    {
        RequestCount = requestCount;
        LastRequestTime = lastRequestTime;
    }

}
