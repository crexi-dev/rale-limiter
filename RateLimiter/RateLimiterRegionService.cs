using RateLimiter.Interface;
using System.Collections.Generic;
using System.Linq;

namespace RateLimiter
{
    public class RateLimiterRegionRuleService : IRateLimiterRegionRuleService
    {
        private readonly IEnumerable<IRateLimiterRule> _rules;
        public RateLimiterRegionRuleService(IEnumerable<IRateLimiterRule> rules) 
        {
            _rules = rules;
        }

        public IEnumerable<IRateLimiterRule> GetRulesByRegion(string region) 
        {
            var regionMatchRules = _rules.Where(x => x.SupportedRegion.Contains(region));
            var rulesApplyToAllRegion = _rules.Where(x => !x.SupportedRegion.Any());
            var rules = regionMatchRules.Concat(rulesApplyToAllRegion).Distinct();
            return rules;
        }
    }
}
