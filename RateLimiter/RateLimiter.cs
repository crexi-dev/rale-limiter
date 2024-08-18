using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RateLimiter.Enums;
using RateLimiter.Interfaces;

namespace RateLimiter
{
    public class RateLimiter
    {
        private readonly List<IRateLimitRule> _rules;

        public RateLimiter(List<IRateLimitRule> rules)
        {
            _rules = rules;
        }

        public async Task<bool> IsRequestAllowedAsync(string accessToken, DateTime requestTime, Region region = Region.ALL_REGIONS)
        {
            var results = await Task.WhenAll(_rules.Select(rule => rule.IsRequestAllowedAsync(accessToken, requestTime, region)));
            bool allRulesAllow = results.All(result => result);

            if (!allRulesAllow)
            {
                return false;
            }

            var recordTasks = _rules.Select(rule => rule.RecordRequest(accessToken, requestTime, region));
            await Task.WhenAll(recordTasks);

            return true;
        }
    }
}
