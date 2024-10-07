using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RateLimiter.Interfaces;
using System.Net;

namespace RateLimiter.Rules;

/// <summary>
/// Abstract Rate Limiter Rule
/// </summary>
public abstract class AbstractRateLimiterRule<T, U> : IRateLimiterRule<U>, IActionFilter, IRateLimiterRule where T : IRateLimiterStorageEntry where U: IRateLimiterResult
{
    #region Constructor
    /// <summary>
    /// The base class of a rule takes the storage as a parameter
    /// </summary>
    /// <param name="storage"></param>
    public AbstractRateLimiterRule(IRateLimiterStorage storage)
    {
        _storage = storage;
    }
    #endregion

    #region Abstracts
    /// <summary>
    /// Whether if the request is allowerd or not
    /// </summary>
    /// <returns></returns>
    public abstract U IsRequestAllowed();
    /// <summary>
    /// Calculates the time in seconds that the client has to wait for another request
    /// </summary>
    /// <param name="rateLimitResult"></param>
    /// <returns></returns>
    protected abstract double CaculateRetryAfter(U rateLimitResult);
    /// <summary>
    /// Gets or create a storage entry using a key
    /// </summary>
    /// <returns></returns>
    protected abstract T GetOrCreateStorageEntry();
    /// <summary>
    /// The key for a storage entry
    /// </summary>
    protected abstract string Key { get; }
    #endregion

    #region Privates
    /// <summary>
    /// Storage
    /// </summary>
    private readonly IRateLimiterStorage _storage;
    /// <summary>
    /// Expected header on the request
    /// </summary>
    private readonly string _accessTokenHeader = Resource1.AccessTokenHeader;
    #endregion

    #region Properties
    protected IRateLimiterStorage Storage => _storage;
    /// <summary>
    /// Parsed token from the expected header
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// IRateLimiterRule implementation
    /// </summary>
    /// <returns></returns>
    IRateLimiterResult IRateLimiterRule.IsRequestAllowed() => IsRequestAllowed();
    #endregion

    #region IActionFilter Implementation
    /// <summary>
    /// not used
    /// </summary>
    /// <param name="context"></param>
    public void OnActionExecuted(ActionExecutedContext context)
    {
    }

    /// <summary>
    /// Sniffs a request and if its not allowed by a given rule, it will return the proper error with the retry-after header
    /// </summary>
    /// <param name="context"></param>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(_accessTokenHeader, out var accessToken) ||
            string.IsNullOrWhiteSpace(accessToken)
        )
        {
            context.Result = new BadRequestObjectResult($"Missing {_accessTokenHeader}");
            return;
        }

        AccessToken = accessToken;

        var result = IsRequestAllowed();

        if (!result.Success)
        {
            context.Result = new ContentResult
            {
                StatusCode = (int)HttpStatusCode.TooManyRequests,
                Content = "Rate limit exceeded.",
                ContentType = "text/plain"
            };

            context.HttpContext.Response.Headers["Retry-After"] = CaculateRetryAfter(result).ToString("F0");

            return;
        }
    }
    #endregion
}
