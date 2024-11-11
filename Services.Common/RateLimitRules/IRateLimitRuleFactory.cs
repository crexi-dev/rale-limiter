using Services.Common.Configurations;

namespace Services.Common.RateLimitRules;

public interface IRateLimitRuleFactory
{
    public IEnumerable<IRateLimitRule> CreateRules(RuleConfig config);
}