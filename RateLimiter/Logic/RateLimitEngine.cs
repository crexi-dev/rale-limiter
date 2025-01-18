using System.Linq;

namespace Crexi.RateLimiter.Logic;

public class RateLimitEngine : IRateLimitEngine
{
    public RateLimitEngineResult CheckClientRequest(
        ClientRequest request,
        ConcurrentQueue<ClientRequest> requests,
        List<RateLimitPolicySettings>? policies)
    {
        if (policies is null || !policies.Any())
        {
            return new RateLimitEngineResult() { IsAllowed = true };
        }
        
        ConcurrentBag<RateLimitPolicyResult> policyResults = new();
        Parallel.ForEach(policies, policy =>
        {
            switch (policy.PolicyType)
            {
                case PolicyType.SlidingWindow:
                    var passed = SlidingWindowEvaluator.CheckRequest(request, requests, policy);
                    policyResults.Add(new RateLimitPolicyResult()
                    {
                        HasPassedPolicy = passed,
                        PolicyName = policy.PolicyName 
                    } );
                    break;
            }
        });

        var result = new RateLimitEngineResult()
        {
            IsAllowed = true,
            PassingPolicyNames = new(),
            FailingPolicyNames = new(),
        };
        
        foreach (var policyResult in policyResults)
        {
            if (policyResult.HasPassedPolicy)
            {
                result.PassingPolicyNames.Add(policyResult.PolicyName);
            }
            else
            {
                result.IsAllowed = false;
                result.FailingPolicyNames.Add(policyResult.PolicyName);
            }
        }

        return result;
    }
}