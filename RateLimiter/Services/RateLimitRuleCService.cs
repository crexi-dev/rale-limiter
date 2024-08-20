using RateLimiter.Dtos;
using RateLimiter.Enums;
using RateLimiter.Interfaces;
using RateLimiter.Options;

namespace RateLimiter.Services;

/// <summary>
/// Service that implements rate-limiting logic for Rule C, which delegates to either Rule A or Rule B based on user locale.
/// </summary>
public class RateLimitRuleCService : IRateLimitRule
{
    private readonly RateLimiterOptions _optionsMonitor;
    private readonly Func<RateLimitRules, IRateLimitRule> _func;

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitRuleCService"/> class.
    /// </summary>
    /// <param name="func">A factory function to retrieve the appropriate rate-limiting service based on the rule.</param>
    public RateLimitRuleCService(Func<RateLimitRules, IRateLimitRule> func)
    {

        _func = func;
    }

    public async Task<bool> IsRequestAllowed(RateLimitRuleRequestDto userInfo)
    {
        bool result = false;
        var userLocale = userInfo.UserLocal.ToLower();// Normalize the user locale to lowercase.

        // Determine if the user's locale is "us" or "eu".
        if (userLocale == "us" || userLocale == "eu")
        {
            // Retrieve the appropriate rate-limiting service based on the user's locale.
            var service = _func(userLocale == "us" ? RateLimitRules.RuleA : RateLimitRules.RuleB);

            // If the service is not null, delegate the rate-limiting check to the corresponding service.
            if (service is not null)
            {
                result =  await service.IsRequestAllowed(userInfo);
            }
        }

        // Return the result of the rate-limiting check.
        return await Task.FromResult(result);

    }
}
