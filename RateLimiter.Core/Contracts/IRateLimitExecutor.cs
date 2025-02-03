using System.Collections.Generic;
using RateLimiter.Enums;
using RateLimiter.Models;

namespace RateLimiter.Contracts;

public interface IRateLimitExecutor
{
    bool Execute(IReadOnlyCollection<RuleType> ruleTypes, Request request);
}