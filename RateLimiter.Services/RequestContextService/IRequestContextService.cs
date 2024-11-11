namespace RateLimiter.Services.RequestContextService;

public interface IRequestContextService
{
    /// <summary>
    /// Returns contextual information for the current request necessary to perform rate limiting.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidRequestContextException"> throws if a valid request context is not configured / injected. </exception>
    Task<RequestContext> GetRequestContext();

    /// <summary>
    /// Updates the configured context for current request.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    Task SetRequestContext(RequestContext context);
}
