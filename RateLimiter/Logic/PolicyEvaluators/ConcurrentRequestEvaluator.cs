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
        var targetLimit = CalculateLimit(request, settings);

        return new RateLimitPolicyResult()
        {
            HasPassedPolicy = activeRequestCount < targetLimit,
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