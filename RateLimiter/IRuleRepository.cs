using System.Threading;
using System.Threading.Tasks;

namespace RateLimiter;

public interface IRuleRepository
{
    Task AddRuleAsync(Rule rule, CancellationToken token = default);
    Task<Rule?> GetRuleAsync(string scope, CancellationToken token = default);
    Task RemoveRuleAsync(string scope, CancellationToken token = default);
}