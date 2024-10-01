using System;
using System.Threading.Tasks;
using RateLimiter.Stores;

namespace RateLimiter.Tests.Common
{
    internal class TestRequestCountStore : IRequestCountStore
    {
        private long requestCountSince;

        internal TestRequestCountStore(long requestCountSince)
        {
            this.requestCountSince = requestCountSince;
        }

        public Task<long> RequestCountSince(string id, DateTimeOffset date)
        {
            return Task.FromResult(this.requestCountSince);
        }

        public void SetRequestCountSince(long requestCountSince)
        {
            this.requestCountSince = requestCountSince;
        }
    }
}
