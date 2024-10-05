namespace RateLimiter.Rules
{
    /// <summary>
    /// An interface that defines a contract for rate limit rules. Each rule will implement this interface.
    /// </summary>
    public interface IRateLimiterRule
    {
        RateLimiterResult CheckLimit(string token, string resource);
    }
}
