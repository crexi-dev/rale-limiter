using Microsoft.Extensions.Caching.Memory;
using RateLimiter.Dtos;
using RateLimiter.Enums;
using RateLimiter.Interfaces;
using RateLimiter.Options;

namespace RateLimiter.Services;

public class RateLimitRuleCService : IRateLimitRule
{
    private readonly IMemoryCache _memoryCache;
    private readonly RateLimiterOptions _optionsMonitor;
    private readonly Func<RateLimitRules, IRateLimitRule> _func;

    public RateLimitRuleCService(Func<RateLimitRules, IRateLimitRule> func)
    {

        _func = func;
    }

    public Task<bool> IsRequestAllowed(RateLimitRuleRequestDto userInfo)
    {
        var userLocale = userInfo.UserLocal.ToLower();

        if (userLocale == "us" || userLocale == "eu")
        {
            var service = _func(userLocale == "us" ? RateLimitRules.RuleA : RateLimitRules.RuleB);
            return Task.FromResult(true);
        }

        return Task.FromResult(false);

    }
}
