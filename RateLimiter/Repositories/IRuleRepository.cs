using RateLimiter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Repositories
{
    public interface IRuleRepository
    {
        Task<IEnumerable<Rule>> GetRulesById(IEnumerable<int> ruleIds);
        Task SaveRules(IEnumerable<Rule> rules);
        Task<int> SaveRule(Rule rule);
    }
}
