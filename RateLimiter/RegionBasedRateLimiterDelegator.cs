using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RateLimiter.Interfaces;
using System;
using System.Net;

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

        Tuple<bool, double> result;

        if (token.StartsWith("US"))
        {
            result = _usRateLimiterRule.Execute(token);
        }
        else
        {
            result = _otherRateLimiterRule.Execute(token);
        }

        if(!result.Item1)
        {
            context.Result = new ContentResult
            {
                StatusCode = (int)HttpStatusCode.TooManyRequests,
                Content = "Rate limit exceeded.",
                ContentType = "text/plain"
            };

            context.HttpContext.Response.Headers["Retry-After"] = result.Item2.ToString("F0");
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
