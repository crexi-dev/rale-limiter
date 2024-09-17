using RateLimiter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Services
{
    public interface IRuleService
    {
        Task<IEnumerable<Rule>> GetRules(IEnumerable<int> ruleIds);
        Task SaveRules(IEnumerable<Rule> rules);
        Task<int> SaveRule(Rule rule);
    }
}
