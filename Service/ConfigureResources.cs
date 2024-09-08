using RateLimiter.Enums;
using RateLimiter.Repositories;
using RateLimiter.Service.Interface;

namespace RateLimiter.Service
{
    public static class ConfigureResources
    {
        public static void Configure(string resource, List<RulesEnum> rules)
        {
            ResourceRules.SaveRules(resource, rules);
        }

        public static IEnumerable<IRule> GetResourceRules(string resource)
        {
            var ruleList = new List<IRule>();
            var rules = ResourceRules.GetRules(resource);

            foreach (var rule in rules)
            {
                switch (rule)
                {
                    case RulesEnum.RuleA:
                        ruleList.Add(new RuleA());
                        break;
                    case RulesEnum.RuleB:
                        ruleList.Add(new RuleB());
                        break;
                    case RulesEnum.RuleC:
                        ruleList.Add(new RuleC());
                        break;
                }
            }

            return ruleList;
        }
    }
}
