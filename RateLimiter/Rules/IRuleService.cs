using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RateLimiter.Rules
{
    public interface IRuleService
    {
        void AddResourceRules(IEnumerable<IRule> rules, IDictionary<string, IEnumerable<string>> resourcexRuleName);
        Task<bool> Allow(string resource, Client client);
    }
}
