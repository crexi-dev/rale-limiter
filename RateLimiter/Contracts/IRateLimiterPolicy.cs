using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace RateLimiter;

public interface IRateLimiterPolicy
{
    /// <summary>
    /// Provides set of rules to apply to particular HTTP request
    /// </summary>
    IEnumerable<IRateLimiterRule> GetRuleset(HttpRequest request);
}
