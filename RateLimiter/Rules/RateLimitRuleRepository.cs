using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RateLimiter.Rules
{
    public class RateLimitRuleRepository : IRateLimitRuleRepository
    {
        public Task<IReadOnlyCollection<RateLimitRuleEntity>> GetRulesByClientId(string clientId)
        {
            // Mock rules - replace with actual DB access logic
            var rules = new List<RateLimitRuleEntity>
            {
                new()
                {
                    ClientId = clientId,
                    Resource = "/api/resource/resourceA",
                    RuleType = RuleType.RequestCount,
                    MaxRequests = 5,
                    TimeSpan = TimeSpan.FromMinutes(1)
                },
                new()
                {
                    ClientId = clientId,
                    Resource = "/api/resource/resourceB",
                    RuleType = RuleType.TimeSpan,
                    RequiredTimeSpan = TimeSpan.FromSeconds(10)
                }
            };

            return Task.FromResult(rules as IReadOnlyCollection<RateLimitRuleEntity>);
        }
    }
}
