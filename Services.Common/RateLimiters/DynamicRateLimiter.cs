using Services.Common.Configurations;
using Services.Common.Models;
using Services.Common.RateLimitRules;

namespace Services.Common.RateLimiters;

public class DynamicRateLimiter : IRateLimiter
{
    private readonly IRuleConfigLoader _configLoader;

    public DynamicRateLimiter(IRuleConfigLoader configLoader)
    {
        _configLoader = configLoader;
    }

    public bool IsRequestAllowed(RateLimitToken token)
    {
        var rules = _configLoader.GetRulesForResource(token.Resource, token.Region);
        return rules.AsParallel().Select(r => r.IsRequestAllowed(token.Id)).All(r => r);
    }
}