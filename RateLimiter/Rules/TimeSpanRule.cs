using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using RateLimiter.Enums;
using RateLimiter.Interfaces;

namespace RateLimiter.Rules
{
    public class TimeSpanRule : IRateLimitRule
    {
        private readonly TimeSpan _timeSpan;
        private readonly MemoryCache _cache;

        public TimeSpanRule(TimeSpan timeSpan)
        {
            _timeSpan = timeSpan;
            _cache = new MemoryCache(new MemoryCacheOptions());
        }

        public async Task<bool> IsRequestAllowedAsync(string accessToken, DateTime requestTime, Region region = Region.ALL_REGIONS)
        {
            bool withinTimespan = _cache.TryGetValue(accessToken, out _); // true = call made within time span limit
            return await Task.FromResult(!withinTimespan);
        }

        public Task RecordRequest(string accessToken, DateTime requestTime, Region region = Region.ALL_REGIONS)
        {
            var absoluteExpiration = requestTime.Add(_timeSpan);
            _cache.Set(accessToken, true, absoluteExpiration);
            return Task.CompletedTask;
        }
    }
}
