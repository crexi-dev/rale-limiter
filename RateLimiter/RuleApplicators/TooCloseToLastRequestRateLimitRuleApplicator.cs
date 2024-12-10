using RateLimiter.Enums;
using RateLimiter.Models;
using RateLimiter.RuleApplicators;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RateLimiter.Rules;

public class TooCloseToLastRequestRateLimitRuleApplicator() : IApplyARateLimit
{
    public RateLimitRules Type => RateLimitRules.TooCloseToLastRequest;

    public RateLimitResult Apply(RateLimitRuleConfiguration configuration, List<DateTime> requests)
    {
        if (requests.Any() && requests?.Last() > DateTime.UtcNow.AddSeconds(-1 * configuration.TooCloseToLastRequestRateLimitRuleMinimumTimeBetweenRequestsInSeconds))
        {
            return new RateLimitResult(RateLimitResultStatuses.Failure, $"Please allow at least {configuration.TooCloseToLastRequestRateLimitRuleMinimumTimeBetweenRequestsInSeconds} seconds between requests.");
        }

        return new RateLimitResult();
    }
}