using System.Collections.Generic;
using System.Linq;
using RateLimiter.Contracts;
using RateLimiter.Enums;
using RateLimiter.Models;

namespace RateLimiter.RateLimitRules;

public class RateLimitRulesExecutor(IEnumerable<IRateLimitRule> existingRules) : IRateLimitExecutor
{
    public bool Execute(IReadOnlyCollection<RuleType> ruleTypes, Request request)
    {
        var applicableRules = existingRules
            .Where(r => r.RegionType == request.RegionType && ruleTypes.Contains(r.RuleType))
            .ToList();

        return applicableRules.All(rule => rule.Validate(request));
    }
}