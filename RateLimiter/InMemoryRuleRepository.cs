using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RateLimiter;

public class InMemoryRuleRepository : IRuleRepository
{
    private readonly HashSet<Rule> _rules = new ();

    public Task AddRuleAsync(Rule rule, CancellationToken token = default)
    {
        if (!_rules.Add(rule))
        {
            throw new RuleAlreadyExistsException(rule);
        }

        return Task.CompletedTask;
    }

    public Task<Rule?> GetRuleAsync(string scope, CancellationToken token = default)
    {
        var rule = _rules.SingleOrDefault(r => r.Scope.Equals(scope));
        return Task.FromResult(rule);
    }

    public Task RemoveRuleAsync(string scope, CancellationToken token = default)
    {
        _rules.RemoveWhere(r => r.Scope.Equals(scope));
        return Task.CompletedTask;
    }
}
