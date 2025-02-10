using RateLimiter.Config;

using System.Collections.Generic;

namespace RateLimiter.Abstractions;

public interface IProvideRateLimitRules
{
    IEnumerable<IDefineARateLimitRule> GetRules(RateLimiterConfiguration config);
}