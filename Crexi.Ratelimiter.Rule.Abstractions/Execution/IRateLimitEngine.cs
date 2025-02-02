using Crexi.RateLimiter.Rule.Model;

namespace Crexi.RateLimiter.Rule.Execution;

public interface IRateLimitEngine
{
    void AddUpdateRules(IEnumerable<RateLimitRule> rules);
    (bool success, int? responseCode) Evaluate(CallData callData);
}