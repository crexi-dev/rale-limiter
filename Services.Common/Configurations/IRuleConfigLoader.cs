using Services.Common.RateLimitRules;

namespace Services.Common.Configurations;

public interface IRuleConfigLoader
{
    IEnumerable<IRateLimitRule> GetRulesForResource(string resource, string region);
}