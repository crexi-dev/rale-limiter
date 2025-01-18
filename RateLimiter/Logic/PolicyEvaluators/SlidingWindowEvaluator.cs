using System.Linq;

namespace Crexi.RateLimiter.Logic;

public class SlidingWindowEvaluator
{
    public static bool CheckRequest(
        ClientRequest request,
        ConcurrentQueue<ClientRequest> requestHistory,
        RateLimitPolicySettings settings)
    {
        if (settings.TimeSpanWindow == null || requestHistory.Count == 0)
        {
            return true;
        }

        if (settings is { ApplyClientTagFilter: true, ClientTagFilters: not null })
        {
            foreach (var (clientTagFilter, filterValues) in settings.ClientTagFilters)
            {
                var requestValue = clientTagFilter switch
                {
                    nameof(ClientRequest.RegionCountryCode) => request.RegionCountryCode,
                    nameof(ClientRequest.SubscriptionLevel) => request.SubscriptionLevel,
                    _ => null
                };

                if (requestValue != null && !filterValues.Contains(requestValue))
                    return true;
            }
        }
        
        TimeSpan window = settings.TimeSpanWindow.Value;
        
        var windowStart = DateTime.Now.Add(-window.Duration());
        var requestsBeforeStart = 0;
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

        return requestHistory.Count - requestsBeforeStart < settings.Limit;
    }
}