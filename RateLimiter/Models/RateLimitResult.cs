using System.Collections.Generic;

namespace RateLimiter.Models;

public class RateLimitResult
{
    public bool IsAllowed { get; set; }
    public IList<string> RulesMessages { get; set; }
}
