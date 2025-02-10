using System;
using System.Collections.Generic;
using RateLimiter.Abstractions;

namespace RateLimiter.Implementations;

public abstract class SpecificRateLimiterRule(Func<string, bool>? selector = null) : IRateLimiterRule
{
    private readonly Func<string, bool> _appliedRule = selector ?? (_ => true);

    public void Validate(string token, IReadOnlyCollection<DateTime> previousRequests)
    {
        if (!_appliedRule(token))
        {
            return;
        }

        ValidateIfRequired(token, previousRequests);
    }

    protected abstract void ValidateIfRequired(string token, IReadOnlyCollection<DateTime> previousRequests);
}