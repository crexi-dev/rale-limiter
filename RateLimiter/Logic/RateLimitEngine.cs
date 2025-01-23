namespace Crexi.RateLimiter.Logic;

/// <summary>
/// Main entrypoint to the library. This class connects request tracking and rate limiting logic
/// </summary>
public class RateLimitEngine : IRateLimitEngine
{
    
    /// <summary>
    /// Main entry point to validate if a request can be allowed
    /// </summary>
    /// <param name="request"></param>
    /// <param name="requestTracker"></param>
    /// <param name="policies"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public RateLimitEngineResult CheckClientRequest(
        ClientRequest request,
        IClientRequestTracker requestTracker,
        IList<RateLimitPolicy>? policies,
        ILogger logger)
    {
        if (policies is null || !policies.Any())
        {
            logger.LogInformation($"No rate limit policies are configured.");
            return new RateLimitEngineResult() { IsAllowed = true };
        }
        
        ConcurrentBag<RateLimitPolicyResult> policyResults = new();
        Parallel.ForEach(policies, policy =>
        {
            switch (policy.PolicyType)
            {
                case PolicyType.SlidingWindow:
                    var requestQueue = requestTracker.GetRequestQueue(request.ClientId);
                    policyResults.Add(SlidingWindowEvaluator.CheckRequest(request, requestQueue, policy));
                    break;

                case PolicyType.ConcurrentRequests:
                    var activeRequestCount = requestTracker.GetActiveRequestCount(request.ClientId);
                    policyResults.Add(ConcurrentRequestEvaluator.CheckRequest(request, activeRequestCount, policy));
                    break;
                
                default:
                    logger.LogCritical($"Policy type {policy.PolicyType} is not supported.");
                    throw new NotSupportedException($"Policy type {policy.PolicyType} is not supported.");
            }
        });

        var result = new RateLimitEngineResult()
        {
            IsAllowed = policyResults.All(a => a.HasPassedPolicy),
            
            PassingPolicyNames = policyResults
                .Where(w => w.HasPassedPolicy)
                .Select(s => s.PolicyName)
                .ToList(),
            
            FailingPolicyNames = policyResults
                .Where(w => w.HasPassedPolicy == false)
                .Select(s => s.PolicyName)
                .ToList(),
        };

        return result;
    }
}