using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace RateLimiter;

public class InMemoryRateLimitStateStorage : IRateLimitStateStorage<int>
{
    private readonly ConcurrentDictionary<int, RateLimitRuleState> _database = new();

    public Task<RateLimitRuleState?> GetStateAsync(int key, CancellationToken token = default)
    {
        if (!_database.TryGetValue(key, out var state))
        {
            return Task.FromResult<RateLimitRuleState?>(null);
        }

        return Task.FromResult(state)!;
    }

    public Task AddOrUpdateStateAsync(int key, RateLimitRuleState state, CancellationToken token = default)
    {
        _database.AddOrUpdate(
            key,
            state,
            (_, _) => state
        );

        return Task.CompletedTask;
    }
}