using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace RateLimiter.Stores
{
    public class InMemoryRequestCountStore : IRequestCountStore
    {
        private const int MaxRequestCount = 100000;

        private readonly IMemoryCache cache;
        private readonly ChannelReader<NewRequest> reader;

        public InMemoryRequestCountStore(
            IMemoryCache cache,
            ChannelReader<NewRequest> reader)
        {
            this.cache = cache;
            this.reader = reader;

            ConsumeReader();
        }

        public Task<long> RequestCountSince(string id, DateTimeOffset date)
        {
            string cacheKey = this.GetCacheKey(id);

            if (this.cache.TryGetValue(cacheKey, out List<DateTimeOffset> entry))
            {
                long count = 0L;
                int i = entry.Count - 1;

                while (i >= 0 && entry[i] >= date)
                {
                    count++;
                    i--;
                }

                return Task.FromResult(count);
            }
            else
            {
                return Task.FromResult(0L);
            }
        }

        private async ValueTask ConsumeReader()
        {
            while (await this.reader.WaitToReadAsync())
            {
                while (reader.TryPeek(out NewRequest? request))
                {
                    if (request == null)
                    {
                        continue;
                    }

                    this.RecordRequest(request);
                    await reader.ReadAsync();
                }
            }
        }

        private void RecordRequest(NewRequest request)
        {
            string cacheKey = this.GetCacheKey(request.ClientId);
            if (this.cache.TryGetValue(cacheKey, out List<DateTimeOffset> entry))
            {
                entry.Add(request.Date);

                if (entry.Count > MaxRequestCount)
                {
                    entry.RemoveAt(0);
                }
            }
            else
            {
                var options = new MemoryCacheEntryOptions() { SlidingExpiration = TimeSpan.FromHours(24) };
                this.cache.Set(cacheKey, new List<DateTimeOffset>() { request.Date }, options);
            }
        }

        private string GetCacheKey(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            return $"RateLimit_Client_{id}";
        }
    }
}
