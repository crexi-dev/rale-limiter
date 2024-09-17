using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using RateLimiter.Models;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Repositories
{
    public class RuleRepository : IRuleRepository
    {
        public ApplicationDBContext _dbContext { get; set; }
        public RuleRepository(ApplicationDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Rule>> GetRulesById(IEnumerable<int> ruleIds)
        {
            return await _dbContext.Rules.Where(x => ruleIds.Contains(x.Id)).Select(x => x).ToListAsync();
        }
        public async Task SaveRules(IEnumerable<Rule> rules)
        {
            await _dbContext.Rules.AddRangeAsync(rules);
            await _dbContext.SaveChangesAsync();  
        }
        public async Task<int> SaveRule(Rule rule)
        {
            await _dbContext.Rules.AddAsync(rule);
            await _dbContext.SaveChangesAsync();
            return rule.Id;
        }
    }
}
