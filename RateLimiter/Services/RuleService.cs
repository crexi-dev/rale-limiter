using RateLimiter.Models;
using RateLimiter.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Services
{
    public class RuleService : IRuleService
    {
        private readonly IRuleRepository _repository;
        public RuleService(IRuleRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Rule>> GetRules(IEnumerable<int> ruleIds)
        {
            return await _repository.GetRulesById(ruleIds);
        }

        public async Task SaveRules(IEnumerable<Rule> rules)
        {
            await _repository.SaveRules(rules);
        }
        public async Task<int> SaveRule(Rule rule)
        {
            var Id = await _repository.SaveRule(rule);
            return Id;
        }
    }
}
