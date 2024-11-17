using M42.Data.Repositories;
using RateLimiter.Data.Interfaces;
using RateLimiter.Data.Models.Data;
using RateLimiter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Services
{
    public class LimiterRulesDataService : IDataService<LimiterRule>
    {
        private readonly DbRepository<LimiterRule> _limiterRulesRepository;

        public LimiterRulesDataService(DbRepository<LimiterRule> limiterRulesRepository)
        {
            _limiterRulesRepository = limiterRulesRepository;
        }

        public async Task<List<LimiterRule>> GetAllAsync()
        {
            string[] includes = new string[] { "" };

            var limiterRules = await _limiterRulesRepository.GetAllAsync(includes);

            return limiterRules;
        }
        public async Task<List<LimiterRule>> FindAsync(BaseModel searchCriteria)
        {
            throw new NotImplementedException();
        }
        public async Task<LimiterRule> SingleAsync(int id)
        {
            string[] includes = new string[] { "" };

            var limiterRule = await _limiterRulesRepository.SingleAsync(id, includes);

            return limiterRule;
        }
        public async Task<LimiterRule?> SingleOrDefaultAsync(int id)
        {
            string[] includes = new string[] { "" };

            var limiterRule = await _limiterRulesRepository.SingleOrDefaultAsync(id, includes);

            return limiterRule;
        }
        public async Task<LimiterRule> SingleAsync(string identifier)
        {
            string[] includes = new string[] { "" };

            var limiterRule = await _limiterRulesRepository.SingleAsync(identifier, includes);

            return limiterRule;
        }
        public async Task<bool> AddAsync(LimiterRule limiterRule)
        {
            var newLimiterRule = await _limiterRulesRepository.AddAsync(limiterRule);

            return true;
        }
        public async Task<bool> UpdateAsync(int id, LimiterRule limiterRule)
        {
            string[] includes = new string[] { "" };

            var existingLimiterRule = await _limiterRulesRepository.SingleAsync(id, includes);

            existingLimiterRule.Name = limiterRule.Name;
            existingLimiterRule.NumPerTimespan = limiterRule.NumPerTimespan;
            existingLimiterRule.NumSeconds = limiterRule.NumSeconds;
            existingLimiterRule.UpdatedBy = limiterRule.UpdatedBy;
            existingLimiterRule.UpdatedDate = DateTime.Now;

            await _limiterRulesRepository.UpdateAsync(existingLimiterRule);

            return true;
        }
        public async Task<bool> RemoveAsync(int id)
        {
            var newLimiterRule = await _limiterRulesRepository.RemoveAsync(id);

            return true;
        }
    }
}
