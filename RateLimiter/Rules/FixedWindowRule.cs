using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RateLimiter.Rules;

/// <summary>
/// Provides a default implementation of Fixed window rule
/// </summary>
/// <param name="window">Defines fixed time window</param>
/// <param name="maxRequests">Defines how many requests during the specified time window is allowed by this rule</param>
/// <param name="storage">Uses storage with default contract to store the data</param>
/// <param name="groupId">used to identify across which group of request this rule should be applied</param>
public class FixedWindowRule(TimeSpan window, int maxRequests, IRateLimiterStorage storage, string groupId) : IRateLimiterRule
{
    private const string KeyPreffix = "fw_";

    public async Task<bool> IsRequestAllowedAsync(HttpRequest request, CancellationToken ct = default)
    {
        if (maxRequests <= 0)
        {
            return false;
        }

        var now = DateTime.UtcNow;

        if (!await storage.TryGetAsync<(int requestsCount, DateTime windowStart)>(GetKey(), out var value, ct) || now - window > value.windowStart)
        {
            await storage.SetAsync(GetKey(), (1, now), window, ct);
            return true;
        }

        if (value.requestsCount < maxRequests)
        {
            await storage.SetAsync(GetKey(), (value.requestsCount + 1, value.windowStart), window, ct);
            return true;
        }

        return false;
    }

    private string GetKey() => $"{KeyPreffix}{groupId}";
}
