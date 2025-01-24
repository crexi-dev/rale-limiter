namespace Crexi.RateLimiter.Logic.PolicyEvaluators;

/// <summary>
/// Processes client filters and returns a limit number which overrides the default limit
/// </summary>
public class ClientFilterEvaluator
{
    public static long CalculateLimit(ClientRequest request, RateLimitPolicy settings)
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