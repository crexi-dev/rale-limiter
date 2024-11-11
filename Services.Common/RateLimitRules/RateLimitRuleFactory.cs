using Services.Common.Configurations;

namespace Services.Common.RateLimitRules;

public class RateLimitRuleFactory : IRateLimitRuleFactory
{
    public IEnumerable<IRateLimitRule> CreateRules(RuleConfig config)
    {
        return config.RuleTypes.Select(t => InstantiateRule(t, config)).ToList();
    }
    
    private IRateLimitRule InstantiateRule(string ruleType, RuleConfig config)
    {
        return ruleType switch
        {
            "RequestPerTimespan" => new RequestPerTimespanRule(config.RequestLimit.Value, config.Timespan.Value),
            "TimeSinceLastCall" => new TimeSinceLastCallRule(config.MinIntervalBetweenRequests.Value),
            _ => throw new ArgumentException($"Unknown rule type: {ruleType}")
        };
    }
}