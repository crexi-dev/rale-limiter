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
            return _rules.Where(x => x.SupportedRegion.Contains(region) || !x.SupportedRegion.Any()).ToList();
        }
    }
}
i