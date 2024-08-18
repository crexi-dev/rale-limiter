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
    public Task<bool> IsRequestAllowed(RateLimitRuleRequestDto userInfo)
    {
        string cacheKey = string.Join(userInfo.UserId.ToString(), RateLimitRules.RuleA.ToString());
        bool result = false;
        if (_memoryCache.TryGetValue(cacheKey, out RuleBDto cacheValue))
        {

            if (cacheValue != null)
            {


                var datetimeNow = DateTime.UtcNow;
                DateTime certainTime = cacheValue.LastCallDateTime.Add(_optionsMonitor.RuleB.MinTimespanBetweenCallsSeconds);
                if (datetimeNow <= certainTime)
                {
                    result = true;

                }
                else
                {
                    cacheValue.LastCallDateTime = datetimeNow;

                    _memoryCache.Set(cacheKey, cacheValue);

                }


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

        return Task.FromResult(result);
    }
}
