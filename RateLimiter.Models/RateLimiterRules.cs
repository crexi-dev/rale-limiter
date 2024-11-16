using RateLimiter.Models.Enums;


namespace Crexi.RulesService.Models;

public class RateLimiterRule
{
    public int Priority { get; set; }
    public string? NextRuleFile { get; set; }    
    public RateTimeRule? MaxRate { get; set; }
    public RateTimeRule? VelocityRate { get; set; }

    public void Populate(Dictionary<string, object> properties)
    {

        
    }


}

public class RateTimeRule
{
    public bool Enable { get;set; }
    public RateSpanTypeEnum RateSpanType { get; set; }
    public double RateSpan { get; set; }
    public double Rate { get; set; }

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
