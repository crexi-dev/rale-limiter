using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using RateLimiter.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace RateLimiter.Rules
{
    public class GeoBasedRule(IDistributedCache cache,
                                    IEnumerable<GeoBasedConfig> configs)
        : IRateLimiterRule
    {
        private readonly IDistributedCache _cache = cache;
        private readonly Dictionary<string, GeoBasedConfig> _configs = configs.ToDictionary(x => x.Country);

        public async Task<bool> EvaluateAsync(HttpContext httpContext)
        {
            // get from token or resolve by ip
            string country = Country.EU;
            if (!_configs.TryGetValue(country, out var config))
            {
                // no limit for current country, so skip it
                return true;
            }

            var request = httpContext.Request;
            var serviceProvider = request.HttpContext.RequestServices;

            string? key = ((IRateLimiterRule)this).GetKey(httpContext);
            if (string.IsNullOrEmpty(key)) return false;

            var cacheKey = $"RateLimit_{nameof(GeoBasedRule)}_{request.Method}_{request.Path}_{key}";
            var cacheValue = await _cache.GetStringAsync(cacheKey);
            if (cacheValue != null)
            {
                return false;
            }

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(config.Seconds)
            };
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(0), options);
            return true;
        }
    }
}
