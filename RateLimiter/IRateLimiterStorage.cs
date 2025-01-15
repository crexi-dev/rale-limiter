using System.Threading;
using System.Threading.Tasks;

namespace RateLimiter;

public interface IRateLimiterStorage
{
    Task<RuleState> GetRuleStateAsync(string scope, CancellationToken token = default);
    Task UpdateStateAsync(string scope, RuleState ruleState, CancellationToken token = default);
}
