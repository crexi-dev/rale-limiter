using RateLimiter.Config;

using System.Collections.Generic;

namespace RateLimiter.Abstractions
{
    public interface IRateLimiterConfigurationValidator
    {
        (bool IsValid, List<string> Errors) Validate(RateLimiterConfiguration configuration);
    }
}
