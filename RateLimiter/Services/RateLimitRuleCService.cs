using RateLimiter.Dtos;
using RateLimiter.Enums;
using RateLimiter.Interfaces;
using RateLimiter.Options;

namespace RateLimiter.Services;

public class RateLimitRuleCService : IRateLimitRule
{
    private readonly RateLimiterOptions _optionsMonitor;
    private readonly Func<RateLimitRules, IRateLimitRule> _func;

    public RateLimitRuleCService(Func<RateLimitRules, IRateLimitRule> func)
    {

        _func = func;
    }

    public async Task<bool> IsRequestAllowed(RateLimitRuleRequestDto userInfo)
    {
        bool result = false;
        var userLocale = userInfo.UserLocal.ToLower();


        if (userLocale == "us" || userLocale == "eu")
        {
            var service = _func(userLocale == "us" ? RateLimitRules.RuleA : RateLimitRules.RuleB);

            if (service is not null)
            {
                result =  await service.IsRequestAllowed(userInfo);
            }
        }


        return await Task.FromResult(result);

    }
}
