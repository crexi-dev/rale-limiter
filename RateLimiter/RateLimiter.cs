using RateLimiter.Rules;
using RateLimiter.Stores;
using System.Linq;

namespace RateLimiter
{
    public class RateLimiter : IRateLimiter
    {
        private readonly IRulesetStore _rulesetStore;

        public RateLimiter(IRulesetStore rulesetStore)
        {
            _rulesetStore = rulesetStore;
        }

        public bool IsRequestAllowed(string resourceId, string userId)
        {
            var applicableRules = _rulesetStore.GetRules(resourceId);
            if (applicableRules == null || !applicableRules.Any())
            {
                return true;
            }

            return applicableRules.All(rule => rule.IsWithinLimit(userId));
        }

        public void RegisterRule(string resourceId, IRateLimitRule rule)
        {
            _rulesetStore.AddRule(resourceId, rule);
        }
    }
}
