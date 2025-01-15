using System.Threading;
using System.Threading.Tasks;

namespace RateLimiter;
public class RuleService
{
    private readonly IRuleRepository _ruleRepository;

    public RuleService(IRuleRepository ruleRepository)
    {
        _ruleRepository = ruleRepository;
    }

    public async Task AddRuleAsync(Rule rule, CancellationToken token = default)
    {
        await _ruleRepository.AddRuleAsync(rule, token);
    }

    public async Task<Rule> GetRuleAsync(string scope, CancellationToken token = default)
    {
        var rule = await _ruleRepository.GetRuleAsync(scope, token);

        return rule ?? new NullRule();
    }

    public async Task RemoveRuleAsync(string scope, CancellationToken token = default)
    {
        await _ruleRepository.RemoveRuleAsync(scope, token);
    }
}
