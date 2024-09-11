using System.Collections.Generic;
using System.Threading.Tasks;

namespace RateLimiter.Rules;

public interface IRateLimitRuleRepository
{
    Task<IReadOnlyCollection<RateLimitRuleEntity>> GetRulesByClientId(string clientId);
}