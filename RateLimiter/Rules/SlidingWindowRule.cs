using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RateLimiter.Rules;

/// <summary>
/// Provides a default implementation of Sliding Window rule
/// </summary>
/// <param name="window">Defines minimal time interval between requests</param>
/// <param name="storage">Uses storage with default contract to store the data</param>
/// <param name="groupId">Used to identify across which group of request this rule should be applied</param>
public class SlidingWindowRule(TimeSpan window, IRateLimiterStorage storage, string groupId) : IRateLimiterRule
{
    private const string KeyPreffix = "sw_";

    public async Task<bool> IsRequestAllowedAsync(HttpRequest request, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        if (await storage.TryGetAsync<DateTime>(GetKey(), out var lasRequestDate, ct))
        {
            if (now - lasRequestDate < window)
            {
                return false;
            }
        }

        await storage.SetAsync(GetKey(), now, window, ct);
        return true;
    }

    private string GetKey() => $"{KeyPreffix}{groupId}";
}
