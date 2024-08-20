using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using RateLimiter.Dtos;
using RateLimiter.Enums;
using RateLimiter.Interfaces;
using RateLimiter.Options;

namespace RateLimiter.Services;

/// <summary>
/// Service that implements rate-limiting logic for Rule A.
/// </summary>
public class RateLimitRuleAService : IRateLimitRule
{
    private readonly IMemoryCacheService _memoryCacheService;
    private readonly RateLimiterOptions _optionsMonitor;
    private readonly MemoryCacheEntryOptions _cacheEntryOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitRuleAService"/> class.
    /// </summary>
    /// <param name="memoryCacheService">The memory cache service for storing rate-limiting data.</param>
    /// <param name="optionsMonitor">The options monitor containing rate-limiting configuration.</param>
    public RateLimitRuleAService(IMemoryCacheService memoryCacheService, IOptionsMonitor<RateLimiterOptions> optionsMonitor)
    {
        _memoryCacheService = memoryCacheService;
        _optionsMonitor = optionsMonitor.CurrentValue;
        _cacheEntryOptions = new MemoryCacheEntryOptions
        {
            Priority = CacheItemPriority.NeverRemove
        };
    }
    /// <summary>
    /// Determines if a request is allowed based on the rate-limiting rules for Rule A.
    /// </summary>
    /// <param name="userInfo">User information to evaluate against the rate-limiting rules.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the request is allowed.</returns>
    public Task<bool> IsRequestAllowed(RateLimitRuleRequestDto userInfo)
    {
        // Construct the cache key using the UserId and the RuleA identifier.
        string cacheKey = string.Concat(userInfo.UserId.ToString(),"_", RateLimitRules.RuleA.ToString());

        bool result = false;

        var cacheValue = _memoryCacheService.Get<RuleADto>(cacheKey);


        if (cacheValue != null)
        {
            // Calculate the time difference since the last request.
            var datetimeNow = DateTime.UtcNow;
            TimeSpan difference = datetimeNow - cacheValue.LastCallDateTime;

            // If the time difference is less than the allowed timespan and the request count exceeds the limit, block the request.
            if (difference < _optionsMonitor.RuleA.TimespanSeconds && cacheValue.RequestCount > _optionsMonitor.RuleA.RequestsPerTimespan)
            {
                result = true; // Request is blocked.

            }
            else
            {
                // Increment the request count if within the allowed timespan.
                cacheValue.RequestCount++;

                _memoryCacheService.Set<RuleADto>(cacheKey, cacheValue);
            }

            // Remove the cache entry if the timespan has been exceeded
            if (difference > _optionsMonitor.RuleA.TimespanSeconds)
            {
                _memoryCacheService.Remove(cacheKey);
            }

        }
        else
        {
            // If no cache entry exists, create a new one with the current request details.
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
