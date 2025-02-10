using RateLimiter.Config;

using System.Collections.Generic;

namespace RateLimiter.Abstractions;

public interface IProvideRateLimitRules
{
    IEnumerable<IDefineRateLimitRules> GetRules(RateLimiterConfiguration config);
}