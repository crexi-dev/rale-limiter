using System;

namespace RateLimiter.Common
{
    public interface ITimeProvider
    {
        DateTimeOffset Now();
    }

    public class SystemTimeProvider : ITimeProvider
    {
        public DateTimeOffset Now() => DateTimeOffset.UtcNow;
    }
}
