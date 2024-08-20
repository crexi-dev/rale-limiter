using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using RateLimiter.Dtos;
using RateLimiter.Enums;
using RateLimiter.Interfaces;
using RateLimiter.Options;

namespace RateLimiter.Services;

/// <summary>

/// Service that implements rate-limiting logic for Rule B.

/// </summary>
public class RateLimitRuleBService : IRateLimitRule
{
    private readonly IMemoryCacheService _memoryCacheService;
    private readonly RateLimiterOptions _optionsMonitor;
    private readonly MemoryCacheEntryOptions _cacheEntryOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitRuleBService"/> class.
    /// </summary>
    /// <param name="memoryCacheService">The memory cache service for storing rate-limiting data.</param>
    /// <param name="optionsMonitor">The options monitor containing rate-limiting configuration.</param>
    public RateLimitRuleBService(IMemoryCacheService memoryCacheService, IOptionsMonitor<RateLimiterOptions> optionsMonitor)
    {
        _memoryCacheService = memoryCacheService;
        _optionsMonitor = optionsMonitor.CurrentValue;
        _cacheEntryOptions = new MemoryCacheEntryOptions
        {
            Priority = CacheItemPriority.NeverRemove
        };
    }


    /// <summary>
    /// Determines if a request is allowed based on the rate-limiting rules for Rule B.
    /// </summary>
    /// <param name="userInfo">User information to evaluate against the rate-limiting rules.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the request is allowed.</returns>
    public Task<bool> IsRequestAllowed(RateLimitRuleRequestDto userInfo)
    {
        // Construct the cache key using the UserId and the RuleB identifier.
        string cacheKey = string.Concat(userInfo.UserId.ToString(), "_", RateLimitRules.RuleB.ToString());
        bool result = false;

        // Attempt to retrieve the cached RuleB data for the user.
        var cacheValue = _memoryCacheService.Get<RuleBDto>(cacheKey);

        if (cacheValue != null)
        {

            // Calculate the current time and the time when the next request is allowed.
            var datetimeNow = DateTime.UtcNow;
            DateTime certainTime = cacheValue.LastCallDateTime.Add(_optionsMonitor.RuleB.MinTimespanBetweenCallsSeconds);
            if (datetimeNow <= certainTime)
            {
                result = true; // Request is blocked.

            }
            else
            {
                cacheValue.LastCallDateTime = datetimeNow;

                _memoryCacheService.Set(cacheKey, cacheValue);

            }


        }
        else
        {
            // If no cache entry exists, create a new one with the current request time.
            var newCacheValue = new RuleBDto
            {
                LastCallDateTime = DateTime.UtcNow
            };

            _memoryCacheService.Set(cacheKey, newCacheValue, _cacheEntryOptions);
        }

        return Task.FromResult(result);
    }
}
