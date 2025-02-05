using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace RateLimiter.Rules
{
    public class FixedWindowRule(IDistributedCache cache, int limit, int seconds) : IRateLimiterRule
    {
        private readonly IDistributedCache _cache = cache;
        private readonly int _limit = limit;
        private readonly TimeSpan _timeSpan = TimeSpan.FromSeconds(seconds);

        public async Task<bool> EvaluateAsync(HttpContext httpContext)
        {
            var request = httpContext.Request;
            var serviceProvider = request.HttpContext.RequestServices;

            string? key = ((IRateLimiterRule)this).GetKey(httpContext);
            if (string.IsNullOrEmpty(key)) return false;

            var cacheKey = $"RateLimit_{nameof(FixedWindowRule)}_{request.Method}_{request.Path}_{key}";
            var cacheValue = await _cache.GetStringAsync(cacheKey);
            int requestCount = cacheValue != null ? JsonSerializer.Deserialize<int>(cacheValue) : 0;
            if (requestCount >= _limit)
            {
                return false;
            }

            requestCount++;
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _timeSpan
            };
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(requestCount), options);
            return true;
        }
    }
}
