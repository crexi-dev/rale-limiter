using RateLimitingLibrary.Core.Models;

namespace RateLimitingLibrary.Core.Interfaces
{
    /// <summary>
    /// Interface for rate limit rule evaluation.
    /// </summary>
    public interface IRateLimitRule
    {
        /// <summary>
        /// Evaluates the client request against the rule.
        /// </summary>
        /// <param name="request">The client request.</param>
        /// <returns>The result of the rate limit evaluation.</returns>
        RateLimitResult Evaluate(ClientRequest request);
    }
}