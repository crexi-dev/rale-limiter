using Ardalis.GuardClauses;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RateLimiter.Rules;

/// <summary>
/// Implements a rate limiting rule based on the maximum number of requests allowed
/// within a specified time window for each client-resource pair.
/// </summary>
public class RequestCountRule(int maxRequests, TimeSpan windowSize) : IRateLimitRule
{
    private readonly int _maxRequests = Guard.Against.NegativeOrZero(maxRequests);
    private readonly TimeSpan _windowSize = Guard.Against.NegativeOrZero(windowSize);

    // Use a ConcurrentDictionary to store request timestamps for each client-resource pair
    private readonly ConcurrentDictionary<string, List<DateTime>> _requestLogs = new();

    /// <summary>
    /// Determines whether a request is allowed based on the maximum number of requests
    /// within a specified time window for a given client-resource pair.
    /// </summary>
    /// <param name="clientId">The unique identifier for the client making the request.</param>
    /// <param name="resource">The resource being accessed by the client.</param>
    public Task<bool> IsRequestAllowed(string clientId, string resource)
    {
        Guard.Against.NullOrWhiteSpace(clientId);
        Guard.Against.NullOrWhiteSpace(resource);

        var key = $"{clientId}:{resource}";
        var utcNow = DateTime.UtcNow;

        var requestTimestamps = _requestLogs.GetOrAdd(key, _ => []);

        lock (requestTimestamps)
        {
            // Remove requests that are outside the sliding window
            requestTimestamps.RemoveAll(timestamp => timestamp <= utcNow - _windowSize);

            if (requestTimestamps.Count >= _maxRequests)
            {
                return Task.FromResult(false);
            }

            requestTimestamps.Add(utcNow);
        }

        return Task.FromResult(true);
    }
}