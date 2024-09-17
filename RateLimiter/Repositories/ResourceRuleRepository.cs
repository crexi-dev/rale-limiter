using Microsoft.EntityFrameworkCore;
using RateLimiter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Repositories
{
    public class ResourceRuleRepository : IResourceRuleRepository
    {
        public ApplicationDBContext _dbContext { get; set; }
        
        public ResourceRuleRepository(ApplicationDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Rule>> GetRulesForResource(int resourceId)
        {
            var ruleIds = await _dbContext.ResourceRules.Where(x => x.ResourceId == resourceId && x.Active == true).Select(a => a.RuleId).ToListAsync();
            return await _dbContext.Rules.Where(x => ruleIds.Contains(x.Id)).ToListAsync();
        }
    }
}
