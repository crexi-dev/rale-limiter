using RateLimiter.Models.Enums;


namespace RulesService.Models;

public class RateLimiterRule
{
    public int Priority { get; set; }
    public string? Name { get;set; }
    public string? NextRuleFile { get; set; }
    public string? NextWorkflow { get; set; }
    public RateTimeRule? MaxRate { get; set; }
    public RateTimeRule? VelocityRate { get; set; }

    public override string ToString()
    {
        return $"Priority : {Priority}, Name : {Name}, MaxRate : {MaxRate},  VelocityRate : {VelocityRate}, NextRuleFile : {NextRuleFile}";
    }

}

public class RateTimeRule
{
    public RateSpanTypeEnum RateSpanType { get; set; }
    public double RateSpan { get; set; }
    public double Rate { get; set; }

    public override string ToString()
    {
        return $"{Rate} per {RateSpan} {RateSpanType}";
    }
}
public class LimitWorkingHoursRule
{
    public bool Enable { get; set; }
    public List<string> Days { get; set; } = new();
    public string  StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public RateLimiterRule? LimitRule { get; set; }

}
public class LimiDateRule
{
    public bool Enable { get; set; }
    public List<DateOnly> Dates { get; set; } = new();
    public LimitWorkingHoursRule? LimitRule { get; set; }

}
