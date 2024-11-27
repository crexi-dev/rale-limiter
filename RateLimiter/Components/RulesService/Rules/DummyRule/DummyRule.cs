using RateLimiter.Components.RuleService;
using RateLimiter.Models;
using System.Threading.Tasks;

namespace RateLimiter.Components.RulesService.Rules.DummyRule
{
    public class DummyRule : IRateLimitingRule
    {
        public virtual Task<bool> RunAsync(RateLimitingRequestData requestData, RateLimitingRuleConfiguration ruleConfig)
        {
            // this is a dummy example of a rule that will always allow everything
            return Task.FromResult(true);
        }
    }
}
