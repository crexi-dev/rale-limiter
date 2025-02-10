using RateLimiter.Models;

namespace RateLimiter.Interfaces;

/// <summary>
/// Interface for RateLimitRule.
/// </summary>
public interface IRateLimitRule 
{
    /// <summary>
    /// Evaluates rate limiting rule. 
    /// </summary>
    /// <param name="context"><see cref="RateLimitContext"/></param>
    /// <returns><see cref="RateLimitResponse"/></returns>
    RateLimitResponse Evaluate(RateLimitContext context);
}