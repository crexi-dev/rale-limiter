using Services.Common.RateLimitRules;

namespace Services.Common.RateLimiters;

public interface IRateLimitRuleFactory
{
    IRateLimitRule CreateRule(string ruleType, params object[] parameters);
}