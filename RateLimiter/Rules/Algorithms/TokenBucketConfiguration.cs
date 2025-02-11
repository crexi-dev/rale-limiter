namespace RateLimiter.Rules.Algorithms
{
    public class TokenBucketConfiguration
    {
        public int MaxTokens { get; set; }

        public int RefillRatePerSecond { get; set; }
    }
}
