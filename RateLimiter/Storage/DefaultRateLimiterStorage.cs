using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using RateLimiter.Rules;

namespace RateLimiter.Storage;

public class DefaultRateLimiterStorage<TKey> : IRateLimiterStorage<TKey>
{
    private readonly ConcurrentDictionary<TKey, IRateLimitRule> _storage = new();

    public ValueTask AddRateLimitRuleAsync(TKey key, IRateLimitRule rateLimitRule, CancellationToken ct = default)
    {
        _storage.GetOrAdd(key, _ => rateLimitRule);

       return ValueTask.CompletedTask;
    }

    ValueTask<IRateLimitRule> IRateLimiterStorage<TKey>.GetRuleAsync(TKey key, CancellationToken ct = default)
    {
        var result = _storage.TryGetValue(key, out var rule) ? rule : null;

        return new ValueTask<IRateLimitRule>(result);
    }
}
