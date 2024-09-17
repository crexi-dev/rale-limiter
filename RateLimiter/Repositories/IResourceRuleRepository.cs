using RateLimiter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Repositories
{
    public interface IResourceRuleRepository
    {
        Task<IEnumerable<Rule>> GetRulesForResource(int resourceId);
    }
}
