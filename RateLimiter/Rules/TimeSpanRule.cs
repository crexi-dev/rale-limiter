using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace RateLimiter.Rules;

/// <summary>
/// Implements a rate limiting rule based on a required time span between consecutive requests
/// for a given client-resource pair.
/// </summary>
public class TimeSpanRule(TimeSpan requiredTimeSpan) : IRateLimitRule
{
    private readonly ConcurrentDictionary<string, DateTime> _lastRequestTime = new();

    /// <summary>
    /// Determines whether a request is allowed based on the required time span between 
    /// consecutive requests for a specific client-resource pair.
    /// 
    /// The method checks the time of the last request made by the client for the specified 
    /// resource. If the required time span has passed since the last request, it updates the 
    /// last request time to the current timestamp and allows the request. If not, the request 
    /// is denied.
    /// </summary>
    /// <param name="clientId">The unique identifier of the client making the request.</param>
    /// <param name="resource">The resource being accessed by the client.</param>
    public Task<bool> IsRequestAllowed(string clientId, string resource)
    {
        var key = $"{clientId}:{resource}";
        var utcNow = DateTime.UtcNow;

        if (!_lastRequestTime.TryGetValue(key, out var value))
        {
            _lastRequestTime[key] = utcNow;
            return Task.FromResult(true);
        }

        var timeSinceLastRequest = utcNow - value;
        if (timeSinceLastRequest >= requiredTimeSpan)
        {
            _lastRequestTime[key] = utcNow;
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }
}