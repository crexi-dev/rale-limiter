using System;
using System.Threading;
using System.Threading.Tasks;

namespace RateLimiter;

public interface IRateLimitStateStorage<in T> where T : IEquatable<T>
{
    Task<RateLimitRuleState?> GetStateAsync(T key, CancellationToken token = default);
    Task AddOrUpdateStateAsync(
        T key,
        RateLimitRuleState state,
        CancellationToken token = default);
}
