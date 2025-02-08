using RateLimiter.Config;

using System.Collections.Generic;

namespace RateLimiter.Abstractions;

public interface IRateLimitRequests
{
    (bool, string) IsRequestAllowed(IEnumerable<RateLimitedResource> rateLimitedResources);
}