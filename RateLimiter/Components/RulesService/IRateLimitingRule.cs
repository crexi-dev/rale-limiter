using RateLimiter.Models;
using System.Threading.Tasks;

namespace RateLimiter.Components.RuleService
{
    public interface IRateLimitingRule
    {
        Task<bool> RunAsync(RateLimitingRequestData requestData, RateLimitingRuleConfiguration ruleConfig);
    }
}
