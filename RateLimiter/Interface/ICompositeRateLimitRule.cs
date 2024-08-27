
namespace RateLimiter.Interface
{
    /// <summary>
    /// Interface contract to indicate the behavior of a composite Rate Limiting rule one whose behavior is composed by its constituent components
    /// </summary>
    public interface ICompositeRateLimitRule : IRateLimitRule
    {
        /// <summary>
        /// Adds this rate limit rule to its collection of rules to evaluate
        /// </summary>
        /// <param name="rateLimitRule">The rate limit rule to add to the collection</param>
        void Add(IRateLimitRule? rateLimitRule);
    }
}

