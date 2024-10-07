using Microsoft.AspNetCore.Mvc.Filters;

namespace RateLimiter.Interfaces;

/// <summary>
/// Interface of a Rate Limiter Rule
/// </summary>
public interface IRateLimiterRule<T> where T : IRateLimiterResult
{
    /// <summary>
    /// An Implemented rule must be able to check if a request is allowed
    /// </summary>
    /// <returns></returns>
    T IsRequestAllowed();
}

/// <summary>
/// Clone that doesn't require the generic parameter
/// </summary>
public interface IRateLimiterRule
{
    IRateLimiterResult IsRequestAllowed();
    void OnActionExecuting(ActionExecutingContext context);
}