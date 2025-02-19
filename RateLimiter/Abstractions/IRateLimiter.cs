using Microsoft.AspNetCore.Http;

using RateLimiter.Config;

using System.Collections.Generic;

namespace RateLimiter.Abstractions;

public interface IRateLimiter
{
    (bool RequestIsAllowed, string ErrorMessage) IsRequestAllowed(HttpContext context, IEnumerable<RateLimitedResource> rateLimitedResources);
}