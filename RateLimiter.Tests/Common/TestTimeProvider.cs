using System;
using RateLimiter.Common;

namespace RateLimiter.Tests.Common
{
    internal class TestTimeProvider : ITimeProvider
    {
        private DateTimeOffset now;

        internal TestTimeProvider(DateTimeOffset time)
        {
            this.now = time;
        }

        public DateTimeOffset CurrentNow => now;

        public DateTimeOffset Now()
        {
            return now;
        }

        public void SetNow(DateTimeOffset now)
        {
            this.now = now;
        }

        public void Advance(TimeSpan timespan)
        {
            this.now = this.now.Add(timespan);
        }
    }
}
