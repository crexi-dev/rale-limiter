using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RateLimiter;

public class InMemoryRateLimiterRuleStorage : IRateLimiterRuleStorage
{
    private readonly HashSet<RateLimitRule> _database = new();
    private readonly object _lock = new();

    public Task AddOrUpdateRuleAsync(RateLimitRule rule, CancellationToken token = default)
    {
        lock (_lock)
        {
            if (!_database.TryGetValue(rule, out var existingRule))
            {
                _database.Add(rule);
            }
            else
            {
                _database.Remove(existingRule);
                _database.Add(rule);
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
            var rule = _database.Where(
                    r => string.Equals(r.Domain, domain, StringComparison.InvariantCultureIgnoreCase))
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
}
