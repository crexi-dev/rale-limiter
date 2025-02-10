using System;
using System.Collections.Generic;
using System.Linq;
using RateLimiter.Interfaces;
using RateLimiter.Rules;
using RateLimiter.Models;

namespace RateLimiter.Builder;

/// <summary>
/// Builder to compile individual and/or groups of rate limit rules.
/// By default, will evaluate multiple rules with logical AND.
/// </summary>
public class RateLimitRuleBuilder 
{
    private readonly List<IRateLimitRule> _rateLimitRules = new List<IRateLimitRule>();  

    /// <summary>
    /// Adds rule to builder.
    /// </summary>
    /// <param name="rule"><see cref="IRateLimitRule"/></param>
    /// <returns><see cref="RateLimitRuleBuilder"/></returns>
    public RateLimitRuleBuilder Add(IRateLimitRule rule) 
    {
        _rateLimitRules.Add(rule);
        return this;
    }

    /// <summary>
    /// Adds a collection of rules that will be evaluated with logical AND. 
    /// </summary>
    /// <param name="rateLimitRules">Collection of rate limit rules.</param>
    /// <returns><see cref="RateLimitRuleBuilder"/></returns>
    public RateLimitRuleBuilder Add(IEnumerable<IRateLimitRule> rateLimitRules)
    {
        _rateLimitRules.Add(new AndCompositeRule(rateLimitRules));
        return this;
    }

    /// <summary>
    /// Adds a collection of rules that will be evaluated with logical OR.
    /// </summary>
    /// <param name="rateLimitRules"></param>
    /// <returns><see cref="RateLimitRuleBuilder"/></returns>
    public RateLimitRuleBuilder Or(IEnumerable<IRateLimitRule> rateLimitRules) 
    {
        _rateLimitRules.Add(new OrCompositeRule(rateLimitRules));
        return this;
    }

    /// <summary>
    /// Adds a rule that should only apply for a specified region.
    /// </summary>
    /// <param name="rule"><see cref="IRateLimitRule"/></param>
    /// <param name="region"><see cref="Region"/></param>
    /// <returns>The current builder instance for fluent chaining.</returns>
    public RateLimitRuleBuilder AddForRegion(IRateLimitRule rule, Region region)
    {
        _rateLimitRules.Add(new RegionBasedRuleDecorator(rule, region));
        return this;
    }

    /// <summary>
    /// Compiles the list of rules into a single rule combined with logical AND.  
    /// </summary>
    /// <returns><see cref="IRateLimitRule"/></returns>
    public IRateLimitRule Build() 
    {
        if (_rateLimitRules.Count == 0) 
        {
            throw new InvalidOperationException("No rate limit rules have been added.");
        }
        else if (_rateLimitRules.Count == 1) 
        {
            return _rateLimitRules.First();
        }
        return new AndCompositeRule(_rateLimitRules);
    }
}