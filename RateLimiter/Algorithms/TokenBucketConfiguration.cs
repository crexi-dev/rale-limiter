using RateLimiter.Abstractions;

namespace RateLimiter.Algorithms
{
    public class TokenBucketConfiguration : IRateLimitAlgorithmConfiguration
    {
        public int MaxTokens { get; set; }

        public int RefillRatePerSecond { get; set; }
    }
}
