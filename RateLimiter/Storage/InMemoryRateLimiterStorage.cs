using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RateLimiter.Domain;

namespace RateLimiter.Storage;

public class InMemoryRateLimiterStorage : IRateLimiterRuleStorage, IRateLimitStateStorage<int>
{
    private readonly HashSet<RateLimitRule> _rulesDatabase = new();
    private readonly ConcurrentDictionary<int, RateLimitRuleState> _stateDatabase = new();

    private readonly object _lock = new();

    public Task AddOrUpdateRuleAsync(RateLimitRule rule, CancellationToken token = default)
    {
        lock (_lock)
        {
            if (!_rulesDatabase.TryGetValue(rule, out var existingRule))
            {
                _rulesDatabase.Add(rule);
            }
            else
            {
                _rulesDatabase.Remove(existingRule);
                _rulesDatabase.Add(rule);
            }
        }

        return Task.CompletedTask;
    }

    public Task<RateLimitRule?> GetRuleAsync(
        string domain,
        RateLimitDescriptor descriptor,
        CancellationToken token = default)
    {
        lock (_lock)
        {
            var rule = _rulesDatabase.Where(
                    r => string.Equals(r.Domain, domain, StringComparisonDefaults.DefaultStringComparison))
                .SingleOrDefault(r => r.Descriptors.Any(d => d.Equals(descriptor)));

            if (rule == null)
            {
                return Task.FromResult<RateLimitRule?>(null);
            }

            if (!rule.Descriptors.Any(d => d.Equals(descriptor)))
            {
                return Task.FromResult<RateLimitRule?>(null);
            }

            return Task.FromResult(rule)!;
        }
    }

    public Task<RateLimitRuleState?> GetStateAsync(int key, CancellationToken token = default)
    {
        if (!_stateDatabase.TryGetValue(key, out var state))
        {
            return Task.FromResult<RateLimitRuleState?>(null);
        }

        return Task.FromResult(state)!;
    }

    public Task AddOrUpdateStateAsync(
        int key,
        RateLimitRuleState state,
        CancellationToken token = default)
    {
        _stateDatabase.AddOrUpdate(
            key,
            state,
            (_, _) => state
        );

        return Task.CompletedTask;
    }
}
