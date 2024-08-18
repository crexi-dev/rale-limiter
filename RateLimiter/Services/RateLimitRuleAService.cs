using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using RateLimiter.Dtos;
using RateLimiter.Enums;
using RateLimiter.Interfaces;
using RateLimiter.Options;

namespace RateLimiter.Services;

public class RateLimitRuleAService : IRateLimitRule
{
    private readonly IMemoryCache _memoryCache;
    private readonly RateLimiterOptions _optionsMonitor;
    private readonly MemoryCacheEntryOptions _cacheEntryOptions;

    public RateLimitRuleAService(IMemoryCache memoryCache, IOptionsMonitor<RateLimiterOptions> optionsMonitor)
    {
        _memoryCache = memoryCache;
        _optionsMonitor = optionsMonitor.CurrentValue;
        _cacheEntryOptions = new MemoryCacheEntryOptions
        {
            Priority = CacheItemPriority.NeverRemove
        };
    }
    public bool IsRequestAllowed(RateLimitRuleRequestDto userInfo)
    {
        string cacheKey = string.Join(userInfo.UserId.ToString(), RateLimitRules.RuleA.ToString());

        bool result = false;
        if (_memoryCache.TryGetValue(cacheKey, out RuleADto cacheValue))
        {


            if (cacheValue != null)
            {

                var datetimeNow = DateTime.UtcNow;
                TimeSpan difference = datetimeNow - cacheValue.LastCallDateTime;
                if (difference.Seconds > _optionsMonitor.RuleA.TimespanSeconds.Seconds && cacheValue.RequestCount == _optionsMonitor.RuleA.RequestsPerTimespan)
                {
                    result = true;
                }

                cacheValue.LastCallDateTime = datetimeNow;
                cacheValue.RequestCount++;

                _memoryCache.Set(cacheKey, cacheValue);

            }
        }
        else
        {

            var newCacheValue = new RuleADto
            {
                LastCallDateTime = DateTime.UtcNow,
                RequestCount = 1
            };

            _memoryCache.Set(cacheKey, newCacheValue, _cacheEntryOptions);

        }

        return result;
    }
}
