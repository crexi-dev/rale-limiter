using RateLimiter.DataStore;
using RateLimiter.Services.Interfaces;
using RateLimiter.Services.Rule;

namespace RateLimiter.Services.Factory
{
    public static class ServiceFactory
    {
        public static IRuleService CreateRuleService(RuleOptions ruleOptions) 
        {
            if (ruleOptions.RuleType == Enums.RuleType.USA)
            {
                return new USARuleService(
                    ruleOptions,
                    new InMemoryDataContext());
            }
            else if (ruleOptions.RuleType == Enums.RuleType.Europe)
            {
                return new EuropeRuleService(
                    ruleOptions,
                    new InMemoryDataContext());
            }
            else if (ruleOptions.RuleType == Enums.RuleType.Mixed)
            {
                return new MixedRuleService(
                    ruleOptions,
                    new InMemoryDataContext());
            }
            else
            {
                throw new Exception("Rule not supported.");
            }
        }
    }
}
