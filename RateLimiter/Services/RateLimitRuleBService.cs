using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using RateLimiter.Dtos;
using RateLimiter.Enums;
using RateLimiter.Interfaces;
using RateLimiter.Options;

namespace RateLimiter.Services;

public class RateLimitRuleBService : IRateLimitRule
{
    private readonly IMemoryCacheService _memoryCacheService;
    private readonly RateLimiterOptions _optionsMonitor;
    private readonly MemoryCacheEntryOptions _cacheEntryOptions;

    public RateLimitRuleBService(IMemoryCacheService memoryCacheService, IOptionsMonitor<RateLimiterOptions> optionsMonitor)
    {
        _memoryCacheService = memoryCacheService;
        _optionsMonitor = optionsMonitor.CurrentValue;
        _cacheEntryOptions = new MemoryCacheEntryOptions
        {
            Priority = CacheItemPriority.NeverRemove
        };
    }
    public Task<bool> IsRequestAllowed(RateLimitRuleRequestDto userInfo)
    {
        string cacheKey = string.Concat(userInfo.UserId.ToString(), "_", RateLimitRules.RuleA.ToString());
        bool result = false;

        var cacheValue = _memoryCacheService.Get<RuleBDto>(cacheKey);

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

                _memoryCacheService.Set(cacheKey, cacheValue);

            }


        }
        else
        {
            var newCacheValue = new RuleBDto
            {
                LastCallDateTime = DateTime.UtcNow
            };

            _memoryCacheService.Set(cacheKey, newCacheValue, _cacheEntryOptions);
        }

        return Task.FromResult(result);
    }
}
