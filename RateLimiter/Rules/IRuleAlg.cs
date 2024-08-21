namespace RateLimiter.Rules
{
    public interface IRuleAlg
    {
        /// Mimics sending a request through the rate limiter rule.
        bool SendRequest();
    }
}