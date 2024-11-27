using RateLimiter.Components.RuleService;
using RateLimiter.Models;

namespace RateLimiter.Components.RulesService.Models
{
    public class SelectRulesResult
    {
        public IRateLimitingRule Rule { get; set; } = default!;
        public RateLimitingRuleConfiguration Configuration { get; set; } = default!;
    }
}
