
namespace RateLimiter
{
    /// <summary>
    /// Interface contract to indicate the behavior of a standard Rate Limiting rule
    /// </summary>
    public interface IRateLimitRule
    {
        /// <summary>
        /// Evaluates the criteria for the specific Rate Limiting rule and allows or denies the request to go through further processing
        /// </summary>
        /// <param name="clientKey">The value of the client specified key to use for the rule evaluation</param>
        /// <returns>True if the request is allowed, otherwise false if the request is denied</returns>
        bool Evaluate(string clientKey);
        /// <summary>
        /// Indicates the number of requests allowed by this rule evaluation  
        /// </summary>
        long Allowed
        {
            get;
        }
        /// <summary>
        /// Indicates the number of requests denied by this rule evaluation  
        /// </summary>
        long Denied
        {
            get;
        }
    }
}

