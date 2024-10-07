using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RateLimiter.Interfaces;

namespace RateLimiter;

/// <summary>
/// Client Tokens that start with US will execute the usRateLimiter
/// Other client Tokens will execute the otherRateLimiter
/// </summary>
public sealed class RegionBasedRateLimiterDelegator : IActionFilter
{
    public RegionBasedRateLimiterDelegator(
        IRateLimiterRule usRateLimiter,
        IRateLimiterRule otherRateLimiter
        ) 
    {
        _usRateLimiterRule = usRateLimiter;
        _otherRateLimiterRule = otherRateLimiter;
    }

    private readonly IRateLimiterRule _usRateLimiterRule;
    private readonly IRateLimiterRule _otherRateLimiterRule;
    private readonly string _accessTokenHeader = Resource1.AccessTokenHeader;

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(_accessTokenHeader, out var accessToken) ||
            string.IsNullOrWhiteSpace(accessToken)
        )
        {
            context.Result = new BadRequestObjectResult($"Missing {_accessTokenHeader}");
            return;
        }

        var token = accessToken.ToString();

        if (token.StartsWith("US"))
        {
            _usRateLimiterRule.OnActionExecuting(context);
        }
        else
        {
            _otherRateLimiterRule.OnActionExecuting(context);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
