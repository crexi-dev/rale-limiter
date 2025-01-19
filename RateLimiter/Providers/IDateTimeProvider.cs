using System;

namespace RateLimiter.Providers
{
    public interface IDateTimeProvider
    {
        DateTime UtcNow { get; }
    }
}
