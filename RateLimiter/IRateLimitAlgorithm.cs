using System.Threading;
using System.Threading.Tasks;
using RateLimiter.Domain;

namespace RateLimiter;
public interface IRateLimitAlgorithm
{
    Task<RateLimitResult> HandleRequestAsync(RateLimitRule rule, CancellationToken token = default);
}
