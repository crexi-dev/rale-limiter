using RateLimiter.Rules;
using System.Collections.Generic;

namespace RateLimiter.Stores
{
    public interface IRulesetStore
    {
        IList<IRateLimitRule> GetRules(string resourceId);
        void AddRule(string resourceId, IRateLimitRule rule);
        void ClearRules(string resourceId);
    }
}
