using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace RateLimiter;

public class InMemoryRateLimiterStorage : IRateLimiterStorage
{
    private readonly Dictionary<string, RuleState> _counters = new();

    public Task<RuleState> GetRuleStateAsync(string scope, CancellationToken token = default)
    {
        return Task.FromResult(_counters.GetValueOrDefault(scope, new RuleState(0)));
    }

    public Task UpdateStateAsync(string scope, RuleState ruleState, CancellationToken token = default)
    {
        _counters.Remove(scope);
        _counters.Add(scope, ruleState);
        return Task.CompletedTask;
    }
}
