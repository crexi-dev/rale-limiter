using RateLimiter.DataStore;
using RateLimiter.Services.Interfaces;
using RateLimiter.Services.Rule;

namespace RateLimiter.Services.Factory
{
    public static class ServiceFactory
    {
        public static IRuleService CreateRuleService(RuleOptions ruleOptions) 
        {
            var reposiotry = new RateLimitRepository(new InMemoryDataContext());

            if (ruleOptions.RuleType == Enums.RuleType.USA)
            {
                return new USARuleService(
                    ruleOptions,
                    reposiotry);
            }
            else if (ruleOptions.RuleType == Enums.RuleType.Europe)
            {
                return new EuropeRuleService(
                    ruleOptions,
                    reposiotry);
            }
            else if (ruleOptions.RuleType == Enums.RuleType.Mixed)
            {
                return new MixedRuleService(
                    ruleOptions,
                    reposiotry);
            }
            else
            {
                throw new Exception("Rule not supported.");
            }
        }
    }
}
