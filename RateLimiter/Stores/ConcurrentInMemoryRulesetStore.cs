using RateLimiter.Rules;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RateLimiter.Stores
{
    public class ConcurrentInMemoryRulesetStore : IRulesetStore
    {
        private readonly ConcurrentDictionary<string, IList<IRateLimitRule>> _rulesetStore;

        public ConcurrentInMemoryRulesetStore()
        {
            _rulesetStore = new ConcurrentDictionary<string, IList<IRateLimitRule>>();
        }

        public ConcurrentInMemoryRulesetStore(ConcurrentDictionary<string, IList<IRateLimitRule>> rulesetStore)
        {
            _rulesetStore = rulesetStore;
        }

        public void AddRule(string resourceId, IRateLimitRule rule)
        {
            var currentRules = _rulesetStore.GetOrAdd(resourceId, new List<IRateLimitRule>());
            currentRules.Add(rule);
        }

        public void ClearRules(string resourceId)
        {
            _rulesetStore.TryRemove(resourceId, out _);
        }

        public IList<IRateLimitRule> GetRules(string resourceId)
        {
            return _rulesetStore.GetOrAdd(resourceId, new List<IRateLimitRule>());
        }
    }
}
