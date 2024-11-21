using RateLimiter.Models.Enums;
using System;
using System.Text.Json.Serialization;


namespace RulesService.Models;

public enum RuleTypeEnum
{
    Base,
    RouterType,
    MaxRateType,
    VelocityType
}



public class RateLimiterRules
{
    public string? Name { get; set; }
    public List<RateLimiterRule> Rules { get; set; } = new ();
}

public class MaxRateTimeSpanRule 
{
    public TimeSpan Timespan { get; set; }
    public double Rate { get; set; }

    public override string ToString()
    {
        return $"{Rate} per {Timespan.TotalSeconds} secs";
    }
}

public class VelocityTimeSpanRule 
{
    //can be MaxRateTimeSpanRule, but I prefer to be explicit
    public TimeSpan Timespan { get; set; }

    public override string ToString()
    {
        return $"1 per {Timespan.TotalSeconds} secs";
    }
}

public class RouterRule 
{
    public string NextRuleFile { get; set; } = null!;
    public string NextWorkflow { get; set; } = null!;

    public override string ToString()
    {
        return $"NextRuleFile : {NextRuleFile}, NextWorkflow : {NextWorkflow}";
    }
}

public class RateLimiterRule
{
    public RuleTypeEnum RuleType { get; set; }
    public int Priority { get; set; }
    public string? Name { get; set; }
    public MaxRateTimeSpanRule? MaxRateRule { get; set; }
    public RouterRule? RouterRule { get; set; }
    public VelocityTimeSpanRule? VelocityRule { get; set; }

    public override string ToString()
    {
        return $" Type : {RuleType}, Priority : {Priority}, Name : {Name}, MaxRateRule : {MaxRateRule},  VelocityRule : {VelocityRule}, RouterRule : {RouterRule}";
    }

}
