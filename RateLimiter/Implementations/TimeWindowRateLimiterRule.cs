using System;
using System.Collections.Generic;
using System.Linq;
using RateLimiter.Exceptions;

namespace RateLimiter.Implementations;

public class TimeWindowRateLimiterRule(TimeSpan timeSpan, int maxRequests, Func<string, bool>? selector = null) : SpecificRateLimiterRule(selector)
{
    protected override void ValidateIfRequired(string token, IReadOnlyCollection<DateTime> previousRequests)
    {
        if (previousRequests.Count(x => DateTime.UtcNow - x <= timeSpan) >= maxRequests)
        {
            throw new RateLimitException("Expected less requests", nameof(TimeWindowRateLimiterRule));
        }
    }
}