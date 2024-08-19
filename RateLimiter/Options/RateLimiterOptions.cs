namespace RateLimiter.Options;

public class RateLimiterOptions
{
    public const string RateLimiter = "RateLimiter";

    public RuleAOptions RuleA { get; set; }
    public RuleBOptions RuleB { get; set; }



}
public class RuleAOptions
{
    public int RequestsPerTimespan { get; set; }
    public TimeSpan TimespanSeconds { get; set; }

}
public class RuleBOptions
{
    public TimeSpan MinTimespanBetweenCallsSeconds { get; set; }

}
