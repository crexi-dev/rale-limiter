using Services.Common.RateLimitRules;

namespace Services.Common.RateLimiters;

public class RateLimiterBase : IRateLimiter
{
    protected readonly List<IRateLimitRule> _rules;

    public RateLimiterBase(List<IRateLimitRule> rules)
    {
        _rules = rules;
    }
    
    public bool IsRequestAllowed(Guid token)  //, string resource
    {
        return _rules.All(rule => rule.IsRequestAllowed(token));
    }
}