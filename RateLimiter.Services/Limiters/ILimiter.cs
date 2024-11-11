namespace RateLimiter.Services.Limiters;

public interface ILimiter
{
    /// <summary>
    /// Peform a specific Rate Limiting Rule for the provided options
    /// </summary>
    /// <returns></returns>
    Task Limit(RateLimitingOptions options, RequestContext context);
}
