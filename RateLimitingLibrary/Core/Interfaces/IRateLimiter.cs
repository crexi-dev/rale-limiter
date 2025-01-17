using RateLimitingLibrary.Core.Models;
using System.Threading.Tasks;

namespace RateLimitingLibrary.Core.Interfaces
{
    /// <summary>
    /// Interface for evaluating requests based on configured rate limiting rules.
    /// </summary>
    public interface IRateLimiter
    {
        /// <summary>
        /// Evaluates the client request and determines if it is allowed.
        /// </summary>
        /// <param name="request">The client request.</param>
        /// <returns>The result of the rate limit evaluation.</returns>
        Task<RateLimitResult> EvaluateRequestAsync(ClientRequest request);
    }
}