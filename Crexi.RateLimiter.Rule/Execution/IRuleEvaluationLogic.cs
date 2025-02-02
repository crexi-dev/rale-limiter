using Crexi.RateLimiter.Rule.Enum;
using Crexi.RateLimiter.Rule.Model;

namespace Crexi.RateLimiter.Rule.Execution;

public interface IRuleEvaluationLogic
{
    (bool success, int? responseCode) EvaluateRule(EvaluationType evaluationType, TimeSpan window, int? maxCallCount, int? overrideResponseCode, CallHistory history);
}