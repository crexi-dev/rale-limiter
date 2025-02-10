using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using RateLimiter.Interfaces;
using RateLimiter.Models;

namespace RateLimiter.Rules;

/// <summary>
/// Rule that defines the minimum time before the next request can be made.
/// </summary>
public class MinimumTimeBetweenRequestsRule : IRateLimitRule 
{
    private readonly TimeSpan _minimumTimeSpan;

    private readonly ConcurrentDictionary<(string clientToken, string resource), DateTime> _timeLastCalled;

    /// <summary>
    /// Initializes an instance of <see cref="MinimumTimeBetweenRequestsRule"./> 
    /// </summary>
    /// <param name="minimumTimeSpan">Minimum time span required before next request.</param>
    public MinimumTimeBetweenRequestsRule(TimeSpan minimumTimeSpan) 
    {
        _minimumTimeSpan = minimumTimeSpan;
        _timeLastCalled = new ConcurrentDictionary<(string clientToken, string resource), DateTime>();
    } 

    /// <summary>
    /// Evaluates if the minimum time has passed from the last call made.
    /// </summary>
    /// <param name="context"><see cref="RateLimitContext"/></param>
    /// <returns><see cref="RateLimitResponse"/></returns>
    public RateLimitResponse Evaluate(RateLimitContext context)
    {
        var requestKey = (context.ClientToken, context.ApiResource);
        var currentTime = DateTime.UtcNow;

        if (_timeLastCalled.TryGetValue(requestKey, out DateTime lastCallTime)
            && ((currentTime - lastCallTime) < _minimumTimeSpan))
        {
            return new RateLimitResponse(false, new List<string> { nameof(MinimumTimeBetweenRequestsRule) });
        }

        _timeLastCalled[requestKey] = currentTime;
        return new RateLimitResponse(true, null);
    }
}