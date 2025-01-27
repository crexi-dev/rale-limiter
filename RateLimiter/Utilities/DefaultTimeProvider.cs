
using System;

namespace RateLimiter.Utilities
{
    public interface ITimeProvider
    {
        DateTime GetCurrentTime();
    }

    public class DefaultTimeProvider : ITimeProvider
    {
        public DateTime GetCurrentTime() => DateTime.UtcNow;
    }
}
