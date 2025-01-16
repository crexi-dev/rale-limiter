using System.Threading;
using System.Threading.Tasks;

namespace RateLimiter;
public interface IRateLimitAlgorithm
{
    Task<RateLimitResult> HandleRequestAsync(
        RateLimitRule rule,
        CancellationToken token = default);
}
