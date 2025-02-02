using Crexi.RateLimiter.Rule.Configuration.Sections;
using Crexi.RateLimiter.Rule.Constants;
using Crexi.RateLimiter.Rule.Enum;
using Crexi.RateLimiter.Rule.Model;
using Microsoft.Extensions.Options;

namespace Crexi.RateLimiter.Rule.Execution;

public class RuleEvaluationLogic(TimeProvider timeProvider, IOptions<RateLimiterConfiguration> configuration) : IRuleEvaluationLogic
{
    public (bool success, int? responseCode) EvaluateRule(EvaluationType evaluationType, TimeSpan window, int? maxCallCount, int? overrideResponseCode, CallHistory history) => evaluationType switch
    {
        EvaluationType.TimespanSinceLastCall => EvaluateTimespanSinceLastCall(window, overrideResponseCode, history),
        EvaluationType.CallsDuringTimespan => EvaluateCallsDuringTimespan(window, maxCallCount, overrideResponseCode, history),
        EvaluationType.WeAreTeasing => EvaluateWeAreTeasing(),
        EvaluationType.WeArePseudoConfusing => EvaluateWeArePseudoConfusing(),
        _ => configuration.Value.UnrecognizedEvaluationTypeEvaluationResult ? ResultConstants.SuccessResponse : (false, configuration.Value.UnrecognizedEvaluationTypeOverrideResponseCode ?? ResultConstants.DefaultFailureResponseCode)
    };

    private (bool success, int? responseCode) EvaluateTimespanSinceLastCall(TimeSpan window,
        int? overrideResponseCode,
        CallHistory history)
    {
        var result =
            timeProvider.GetUtcNow().DateTime.Subtract(window) > history.LastCall
                ? ResultConstants.SuccessResponse
                : (false, overrideResponseCode ?? ResultConstants.DefaultFailureResponseCode);
        return result;
    }

    private (bool success, int? responseCode) EvaluateCallsDuringTimespan(TimeSpan window, int? maxCallCount, int? overrideResponseCode, CallHistory history)
    {
        if (history.Calls is null || history.Calls.Length == 0) return ResultConstants.SuccessResponse;
        var measure = timeProvider.GetUtcNow().DateTime.Subtract(window);
        var windowCallCount = history.Calls.Count(c => c >= measure);
        /*
            NOTE: not handling null call count.  I'd handle it via config values in a real implementation, but here assuming rule validation will prevent errors here.  
        */
        return windowCallCount < maxCallCount! ? ResultConstants.SuccessResponse
            : (false, overrideResponseCode ?? ResultConstants.DefaultFailureResponseCode);
    }

    private static (bool success, int? responseCode) EvaluateWeAreTeasing() => (false, 406);

    private static (bool success, int? responseCode) EvaluateWeArePseudoConfusing() => (new Random().Next() %  2 == 0, 418);
}