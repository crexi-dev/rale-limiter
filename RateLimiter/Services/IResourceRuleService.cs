using RateLimiter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Services
{
    public interface IResourceRuleService
    {
        Task<IEnumerable<Rule>> GetRulesForResource(int resourceId);
    }
}
