using RateLimiter.Storages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RateLimiter.Rules;
public class CompositeRule(IEnumerable<IRateLimitRule> rules) : IRateLimitRule
{
    public async Task<bool> IsRequestAllowedAsync(string clientId, string actionKey, IRateLimitStore store)
    {
        foreach (var rule in rules)
        {
            if (!await rule.IsRequestAllowedAsync(clientId, actionKey, store))
            {
                return false;
            }
        }
        return true;
    }
}