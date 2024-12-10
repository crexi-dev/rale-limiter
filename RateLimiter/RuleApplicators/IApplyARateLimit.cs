using RateLimiter.Enums;
using RateLimiter.Models;
using System;
using System.Collections.Generic;

namespace RateLimiter.RuleApplicators;

public interface IApplyARateLimit
{
    RateLimitRules Type { get; }

    /// <summary>
    /// Apply the rate limit rule
    /// </summary>
    /// <param name="configuration">The configuration data to use to apply the rule</param>
    /// <param name="requests">The list of request timestamps to use when evaluating the rule</param>
    /// <returns>An instance of <see cref="RateLimitResult"/> indicating success or failure, including an optional failure message</returns>
    RateLimitResult Apply(RateLimitRuleConfiguration configuration, List<DateTime> requests);
}