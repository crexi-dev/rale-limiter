using System;

namespace RateLimiter.Abstractions;

public interface IDateTimeProvider
{
    DateTime UtcNow();
}