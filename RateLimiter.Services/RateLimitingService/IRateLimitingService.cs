namespace RateLimiter.Services.RateLimitingService;
using RateLimiter.Common.Exceptions;

public interface IRateLimitingService
{
    /// <summary>
    /// Handles the current scoped request for a resource. 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="RateLimitExceededException"> throws if the request exceeds the limits for the current request resource / token </exception>
    /// <exception cref="InvalidRequestContextException"> throws if a valid request context is not provided / injected. </exception>
    Task HandleRequest();
}
