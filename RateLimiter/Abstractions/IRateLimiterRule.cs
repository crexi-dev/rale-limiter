using System;
using System.Collections.Generic;

namespace RateLimiter.Abstractions;

public interface IRateLimiterRule
{
    void Validate(string token, IReadOnlyCollection<DateTime> previousRequests);
}