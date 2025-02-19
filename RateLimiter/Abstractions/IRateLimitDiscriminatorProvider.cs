using RateLimiter.Config;

using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RateLimiter.Abstractions
{
    public interface IRateLimitDiscriminatorProvider
    {
        ConcurrentDictionary<string, IRateLimitDiscriminator> GenerateDiscriminators(
            List<RateLimiterConfiguration.DiscriminatorConfiguration> configDiscriminators);
    }
}
