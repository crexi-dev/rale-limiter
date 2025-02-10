using System;
using System.Collections.Generic;
using System.Linq;
using RateLimiter.Exceptions;

namespace RateLimiter.Implementations;

public class DelayRateLimiterRule(TimeSpan timeSpan, Func<string, bool>? selector = null) : SpecificRateLimiterRule(selector)
{
    protected override void ValidateIfRequired(string token, IReadOnlyCollection<DateTime> previousRequests)
    {
        if (previousRequests.Any(x => x + timeSpan >= DateTime.UtcNow))
        {
            throw new RateLimitException("Not enough Time since last request", nameof(DelayRateLimiterRule));
        }
    }
}