namespace Crexi.RateLimiter.Logic.PolicyEvaluators;

/// <summary>
/// Logic only / stateless class that checks if the current client has exceeded concurrent request limits
/// </summary>
public static class ConcurrentRequestEvaluator
{
    /// <summary>
    /// Main entry point to evaluate if a client request exceeds concurrency rate limits
    /// </summary>
    /// <param name="request"></param>
    /// <param name="activeRequestCount"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    public static RateLimitPolicyResult CheckRequest(
        ClientRequest request,
        long activeRequestCount,
        RateLimitPolicy settings)
    {
        var targetLimit = ClientFilterEvaluator.CalculateLimit(request, settings);

        return new RateLimitPolicyResult()
        {
            HasPassedPolicy = activeRequestCount < targetLimit,
            PolicyName = settings.PolicyName
        };
    }
}