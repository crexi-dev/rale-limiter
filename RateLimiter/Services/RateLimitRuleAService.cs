using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using RateLimiter.Dtos;
using RateLimiter.Enums;
using RateLimiter.Interfaces;
using RateLimiter.Options;

namespace RateLimiter.Services;

public class RateLimitRuleAService : IRateLimitRule
{
    private readonly IMemoryCacheService _memoryCacheService;
    private readonly RateLimiterOptions _optionsMonitor;
    private readonly MemoryCacheEntryOptions _cacheEntryOptions;

    public RateLimitRuleAService(IMemoryCacheService memoryCacheService, IOptionsMonitor<RateLimiterOptions> optionsMonitor)
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
        string cacheKey = string.Concat(userInfo.UserId.ToString(),"_", RateLimitRules.RuleA.ToString());

        bool result = false;

        var cacheValue = _memoryCacheService.Get<RuleADto>(cacheKey);


        if (cacheValue != null)
        {

            var datetimeNow = DateTime.UtcNow;
            TimeSpan difference = datetimeNow - cacheValue.LastCallDateTime;


            if (difference < _optionsMonitor.RuleA.TimespanSeconds && cacheValue.RequestCount > _optionsMonitor.RuleA.RequestsPerTimespan)
            {
                result = true;

            }
            else
            {
                cacheValue.RequestCount++;

                _memoryCacheService.Set<RuleADto>(cacheKey, cacheValue);
            }


            if (difference > _optionsMonitor.RuleA.TimespanSeconds)
            {
                _memoryCacheService.Remove(cacheKey);
            }

        }
        else
        {

            var newCacheValue = new RuleADto
            {
                LastCallDateTime = DateTime.UtcNow,
                RequestCount = 1
            };

            _memoryCacheService.Set(cacheKey, newCacheValue, _cacheEntryOptions);

        }

        return Task.FromResult(result);
    }
}
