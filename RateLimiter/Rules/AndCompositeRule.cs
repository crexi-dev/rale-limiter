using System.Collections.Generic;
using System.Linq;
using RateLimiter.Interfaces;
using RateLimiter.Models;

namespace RateLimiter.Rules;

/// <summary>
/// Rule combining multiple rules and requires all to pass. (logical AND) 
/// </summary>
public class AndCompositeRule : IRateLimitRule
{
    private readonly List<IRateLimitRule> _rateLimitRules;

    /// <summary>
    /// Initializes an instance of <see cref="AndCompositeRule"/> .
    /// </summary>
    /// <param name="rateLimitRules">Collection of <see cref="IRateLimitRule"/></param>
    public AndCompositeRule(IEnumerable<IRateLimitRule> rateLimitRules) 
    {
        _rateLimitRules = rateLimitRules.ToList();
    }

    /// <summary>
    /// Evaluates a collection of rules using logical AND.
    /// </summary>
    /// <param name="context"><see cref="RateLimitContext"/></param>
    /// <returns><see cref="RateLimitResponse"/></returns>
    public RateLimitResponse Evaluate(RateLimitContext context) 
    {
        var rejectedReasons = new List<string>();

        foreach (var rule in _rateLimitRules) 
        {
            var response = rule.Evaluate(context);
            if (!response.Allowed) 
            {
                rejectedReasons.AddRange(response.RejectedReasons);
            }
        }

        return new RateLimitResponse(!rejectedReasons.Any(), rejectedReasons);
    }
}