using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RateLimiter.Models;
using RateLimiter.Rules;
using RateLimiter.Stores;

namespace RateLimiter
{
    public class RateLimiter : IRateLimiter
    {
        private readonly IRulesetStore _rulesetStore;
        private readonly ILogger<RateLimiter> _logger;
        private readonly bool _allowRequestsOnFailure;

        public RateLimiter(IRulesetStore rulesetStore, ILogger<RateLimiter> logger, bool allowRequestsOnFailure)
        {
            _rulesetStore = rulesetStore;
            _logger = logger;
            _allowRequestsOnFailure = allowRequestsOnFailure;
        }

        public async Task<bool> IsRequestAllowedAsync(RequestModel request)
        {
            try
            {
                var applicableRules = _rulesetStore.GetRules(request.RequestPath);
                if (applicableRules == null || !applicableRules.Any())
                {
                    return true;
                }

                foreach (var rule in applicableRules)
                {
                    if (!await rule.IsWithinLimitAsync(request))
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception e) 
            {
                _logger.LogError(e, e.Message);
            }

            return _allowRequestsOnFailure;
        }

        public void RegisterRule(string resourceId, IRateLimitRule rule)
        {
            _rulesetStore.AddRule(resourceId, rule);
        }
    }
}
