using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using RateLimiter.Dtos;
using RateLimiter.Enums;
using RateLimiter.Interfaces;
using RateLimiter.Options;

namespace RateLimiter.Services;

public class RateLimitRuleBService : IRateLimitRule
{
    private readonly IMemoryCache _memoryCache;
    private readonly RateLimiterOptions _optionsMonitor;
    private readonly MemoryCacheEntryOptions _cacheEntryOptions;

    public RateLimitRuleBService(IMemoryCache memoryCache, IOptionsMonitor<RateLimiterOptions> optionsMonitor)
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
        bool result = true;
        if (_memoryCache.TryGetValue(cacheKey, out RuleBDto cacheValue))
        {

            if (cacheValue != null)
            {


                var datetimeNow = DateTime.UtcNow;
                TimeSpan difference = datetimeNow - cacheValue.LastCallDateTime;
                if (difference.Seconds == _optionsMonitor.RuleB.MinTimespanBetweenCallsSeconds.Seconds)
                {
                    result = false;
                }

                cacheValue.LastCallDateTime = datetimeNow;

                _memoryCache.Set(cacheKey, cacheValue);

            }

        }
        else
        {
            var newCacheValue = new RuleBDto
            {
                LastCallDateTime = DateTime.UtcNow
            };

            _memoryCache.Set(cacheKey, newCacheValue, _cacheEntryOptions);
        }

        return result;
    }
}
