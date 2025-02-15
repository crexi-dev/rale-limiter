using RateLimiter.Config;

using System.Collections.Concurrent;

namespace RateLimiter.Abstractions
{
    public interface IRateLimitAlgorithmProvider
    {
        ConcurrentDictionary<string, IRateLimitAlgorithm>
            GenerateAlgorithmsFromRules(RateLimiterConfiguration configuration);
    }
}
