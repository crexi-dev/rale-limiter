namespace Crexi.RateLimiter.Logic.PolicyEvaluators;

/// <summary>
/// Logic only class determine if a client request exceeds limits within the last X timespan
/// </summary>
public static class SlidingWindowEvaluator
{
    /// <summary>
    /// Main entrypoint for evaluating if a request exceeds limits within a sliding time span window
    /// </summary>
    /// <param name="request"></param>
    /// <param name="requestHistory"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    public static RateLimitPolicyResult CheckRequest(
        ClientRequest request,
        ConcurrentQueue<ClientRequest> requestHistory,
        RateLimitPolicy settings)
    {
        var targetLimit = CalculateLimit(request, settings);

        // If requests under the limit, no need to process further
        if (requestHistory.Count < targetLimit || settings.TimeSpanWindow == null)
        {
            return new RateLimitPolicyResult()
            {
                HasPassedPolicy = true,
                PolicyName = settings.PolicyName
            };
        }
        
        var windowStart = DateTime.UtcNow.Add(-settings.TimeSpanWindow.Value.Duration()); 
        long requestsBeforeStart = 0;
        
        foreach (var req in requestHistory.ToList())
        {
            if (windowStart < req.RequestTime)
            {
                requestsBeforeStart++;
            }
            else
            {
                break;
            }
        }

        return new RateLimitPolicyResult()
        {
            HasPassedPolicy =  (requestHistory.Count - requestsBeforeStart) > targetLimit,
            PolicyName = settings.PolicyName
        }; 
    }

    private static long CalculateLimit(ClientRequest request, RateLimitPolicy settings)
    {
        if (!settings.ApplyClientTagFilter)
        {
            return settings.Limit;
        }

        var overrideLimit = settings.Limit;
        foreach (var clientFilterGroup in settings.ClientFilterGroups)
        {
            var allFiltersMatch = true;
            foreach (var clientFilter in clientFilterGroup.ClientFilters)
            {
                var clientValue = clientFilter.PropertyName switch
                {
                    nameof(ClientRequest.RegionCountryCode) => request.RegionCountryCode,
                    nameof(ClientRequest.SubscriptionLevel) => request.SubscriptionLevel,
                    _ => null
                };

                if (clientValue != clientFilter.TargetValue)
                {
                    allFiltersMatch = false;
                    break;
                }
            }

            if (allFiltersMatch)
            {
                overrideLimit = clientFilterGroup.LimitOverride;
                break;
            }
        }

        return overrideLimit;
    }
}