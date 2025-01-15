using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RateLimiter;
public class RateLimiterService
{
    private readonly IRuleRepository _ruleRepository;

    public RateLimiterService(IRuleRepository ruleRepository)
    {
        _ruleRepository = ruleRepository;
    }

    public async Task<RateLimitResult> ProcessRequestAsync(RateLimitRequest request, CancellationToken token = default)
    {
        var rule = await _ruleRepository.GetRuleAsync(request.Scope, token);

        if (rule == null)
        {
            var errors = new List<string> { $"The rule with scope {request.Scope} does not exist." };
            var result = new RateLimitResult(false, errors);
        }

        return new RateLimitResult();
    }
}
