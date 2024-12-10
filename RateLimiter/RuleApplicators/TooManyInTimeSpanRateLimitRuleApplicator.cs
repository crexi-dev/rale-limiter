using RateLimiter.Enums;
using RateLimiter.Models;
using RateLimiter.RuleApplicators;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RateLimiter.Rules;

public class TooManyInTimeSpanRateLimitRuleApplicator() : IApplyARateLimit
{
    public RateLimitRules Type => RateLimitRules.TooManyInTimeSpan;

    public RateLimitResult Apply(RateLimitRuleConfiguration configuration, List<DateTime> requests)
    {
        if (requests.Count(r => r >= DateTime.UtcNow.AddSeconds(-1 * configuration.TooManyInTimeSpanRateLimitRuleTimeSpanInSeconds)) > configuration.TooManyInTimeSpanRateLimitRuleMaximumRequestsInTimeSpan)
        {
            return new RateLimitResult(RateLimitResultStatuses.Failure, $"Only {configuration.TooManyInTimeSpanRateLimitRuleMaximumRequestsInTimeSpan} requests may be made within {configuration.TooManyInTimeSpanRateLimitRuleTimeSpanInSeconds} seconds.");
        }

        return new RateLimitResult();
    }
}