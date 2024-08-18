using System;

namespace RateLimiter.Options;

public class RateLimiterOptions
{
    public const string RateLimiter = "RateLimiter";

    public RuleA RuleA { get; set; }
    public RuleB RuleB { get; set; }



}
public class RuleA
{
    public int RequestsPerTimespan { get; set; }
    public TimeSpan TimespanSeconds { get; set; }

}
public class RuleB
{
    public TimeSpan MinTimespanBetweenCallsSeconds { get; set; }

}
