using RateLimiter.Abstractions;

using System;

namespace RateLimiter.Common;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow()
    {
        return DateTime.UtcNow;
    }
}