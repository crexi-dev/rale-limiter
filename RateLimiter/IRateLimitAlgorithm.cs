using System.Threading;
using System.Threading.Tasks;

namespace RateLimiter;

public interface IRateLimitAlgorithm
{
    Task<RateLimitResult> ExecuteAsync(Rule rule, CancellationToken token = default);
}
