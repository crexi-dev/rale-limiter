using RateLimiter.Models;
using RateLimiter.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Services
{
    public class ResourceRuleService : IResourceRuleService
    {
        private readonly IResourceRuleRepository _repository;

        public ResourceRuleService(IResourceRuleRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Rule>> GetRulesForResource(int resourceId)
        {
            return await _repository.GetRulesForResource(resourceId);
        }
    }
}
