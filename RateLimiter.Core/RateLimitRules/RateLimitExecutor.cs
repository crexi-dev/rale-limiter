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
        return existingRules
            .Where(r => ruleTypes.Contains(r.RuleType))
            .All(rule => rule.Validate(request));
    }
}