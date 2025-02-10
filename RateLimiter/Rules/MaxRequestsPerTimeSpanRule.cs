using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using RateLimiter.Interfaces;
using RateLimiter.Models;

namespace RateLimiter.Rules;

/// <summary>
/// Rule utilizing a simple fixed window technique to determine if a client has exceeded the maximum requests per time span.
/// </summary>
public class MaxRequestsPerTimeSpanRule : IRateLimitRule 
{
    private readonly int _maxRequests;
    private readonly TimeSpan _timeSpan;
    private readonly ConcurrentDictionary<(string clientToken, string resource), RateLimitWindow> _timeLastCalled;

    /// <summary>
    /// Initializes an instance of <see cref="MaxRequestsPerTimeSpanRule"./> 
    /// </summary>
    /// <param name="maxRequests">Maximum number of requests.</param>
    /// <param name="timeSpan">Time span through which a number of requests is accepted.</param>
    public MaxRequestsPerTimeSpanRule(int maxRequests, TimeSpan timeSpan) 
    {
        _maxRequests = maxRequests;
        _timeSpan = timeSpan;
        _timeLastCalled = new ConcurrentDictionary<(string clientToken, string resource), RateLimitWindow>();
    }

    /// <summary>
    /// Evaluates whether a request is allowed based on number of requests in a time span.
    /// </summary>
    /// <param name="context"><see cref="RateLimitContext"/></param>
    /// <returns><see cref="RateLimitResponse"/></returns>
    public RateLimitResponse Evaluate(RateLimitContext context)
    {
        var requestKey = (context.ClientToken, context.ApiResource);
        DateTime currentTime = DateTime.UtcNow;

        var window = _timeLastCalled.GetOrAdd(requestKey, _ => new RateLimitWindow(currentTime, currentTime + _timeSpan, 0));
        lock (window)
        {
            // Previous time window passed, we can restart the window for this request.
            if (currentTime - window.StartTime >= _timeSpan)
            {
                window.StartTime = currentTime;
                window.EndTime = currentTime + _timeSpan;
                window.RequestCount = 0;
            }

            window.RequestCount++;

            if (window.RequestCount > _maxRequests)
            {
                return new RateLimitResponse(false, new List<string> { nameof(MaxRequestsPerTimeSpanRule) });
            }

            return new RateLimitResponse(true, null);
        }
    }
}