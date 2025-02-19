using RateLimiter.Abstractions;

namespace RateLimiter.Rules.Algorithms
{
    public class TokenBucketConfiguration : IRateLimitAlgorithmConfiguration
    {
        public int MaxTokens { get; set; }

        public int RefillRatePerSecond { get; set; }
    }
}
