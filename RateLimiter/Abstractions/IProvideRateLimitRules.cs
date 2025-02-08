using System.Collections.Generic;

namespace RateLimiter.Abstractions;

public interface IProvideRateLimitRules
{
    IEnumerable<IRateLimitRule> GetRules();
}