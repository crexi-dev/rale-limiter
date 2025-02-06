using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace RateLimiter.Rules
{
    public class CooldownRule(IDistributedCache cache, int seconds) : IRateLimiterRule
    {
        private readonly IDistributedCache _cache = cache;
        private readonly TimeSpan _timeSpan = TimeSpan.FromSeconds(seconds);

        public async Task<bool> EvaluateAsync(HttpContext httpContext)
        {
            var request = httpContext.Request;
            var serviceProvider = request.HttpContext.RequestServices;

            string? key = ((IRateLimiterRule)this).GetKey(httpContext);
            if (string.IsNullOrEmpty(key)) return false;

            var cacheKey = $"RateLimit_{nameof(CooldownRule)}_{request.Method}_{request.Path}_{key}";
            var cacheValue = await _cache.GetStringAsync(cacheKey);
            if (cacheValue != null)
            {
                return false;
            }

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _timeSpan
            };
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(0), options);
            return true;
        }
    }
}
