using System.Collections.Generic;
using System.Linq;
using RateLimiter.Interfaces;
using RateLimiter.Models;

namespace RateLimiter.Rules;

/// <summary>
/// Rule combining multiple rules and requires at least one to pass. (logical OR) 
/// </summary>
public class OrCompositeRule : IRateLimitRule
{
    private readonly List<IRateLimitRule> _rateLimitRules;

    /// <summary>
    /// Initializes an instance of <see cref="OrCompositeRule"/> .
    /// </summary>
    /// <param name="rateLimitRules">Collection of <see cref="IRateLimitRule"/></param>
    public OrCompositeRule(IEnumerable<IRateLimitRule> rateLimitRules) 
    {
        _rateLimitRules = rateLimitRules.ToList();
    }

    /// <summary>
    /// Evaluates a collection of rules using logical OR.
    /// </summary>
    /// <param name="context"><see cref="RateLimitContext"/></param>
    /// <returns><see cref="RateLimitResponse"/></returns>
    public RateLimitResponse Evaluate(RateLimitContext context) 
    {
        var rejectedReasons = new List<string>();
        var allowed = false;

        foreach (var rule in _rateLimitRules) 
        {
            var response = rule.Evaluate(context);
            if (response.Allowed) 
            {
                allowed = response.Allowed;
            }
            else
            {
                rejectedReasons.AddRange(response.RejectedReasons);
            }
        }

        if (allowed)
        {
            return new RateLimitResponse(true, null);
        }

        return new RateLimitResponse(false, rejectedReasons);
    }
}