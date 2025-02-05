using Microsoft.AspNetCore.Http;
using RateLimiter.Rules;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RateLimiter.Policy
{
    public class RateLimitPolicy()
    {
        private readonly List<IRateLimiterRule> _rateLimiters = [];

        public void AddRule(IRateLimiterRule rule)
        {
            _rateLimiters.Add(rule);
        }

        public async Task<bool> EvaluateAllAsync(HttpContext httpContext)
        {
            // Use Task.WhenAny() if evaluation is long-running

            foreach (var rule in _rateLimiters)
            {
                if (!await rule.EvaluateAsync(httpContext))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
