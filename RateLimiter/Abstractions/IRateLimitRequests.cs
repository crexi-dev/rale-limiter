using RateLimiter.Config;

using System.Collections.Generic;

namespace RateLimiter;

public interface IRateLimitRequests
{
    (bool, string) IsRequestAllowed(IEnumerable<RateLimited> rules);
}