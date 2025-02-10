using RateLimiter.Interfaces;
using RateLimiter.Models;

namespace RateLimiter.Rules;

/// <summary>
/// Decorator to check if rule applies to the specified region. 
/// </summary>
public class RegionBasedRuleDecorator : RateLimitRuleDecorator
{
    private readonly Region _region;

    /// <summary>
    /// Initializes an instance of the <see cref="RegionBasedRuleDecorator"/>.
    /// </summary>
    /// <param name="rule"><see cref="IRateLimitRule"/></param>
    /// <param name="region"><see cref="Region"/></param>
    public RegionBasedRuleDecorator(IRateLimitRule rule, Region region)
        : base(rule)
    {
        _region = region;
    }

    /// <summary>
    /// Checks if context region matches the assigned region, and then evaluates the rule.
    /// If context region does not match, then we ignore the rule and return allowed.
    /// </summary>
    /// <param name="context"><see cref="RateLimitContext"/></param>
    /// <returns><see cref="RateLimitResponse"/></returns>
    /// <exception cref="System.NotImplementedException"></exception>
    public override RateLimitResponse Evaluate(RateLimitContext context)
    {
        if (context.Region == _region) 
        {
            return _rateLimitRule.Evaluate(context);
        }

        return new RateLimitResponse(true, null);
    }
}