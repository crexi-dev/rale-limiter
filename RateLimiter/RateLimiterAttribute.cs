using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using Microsoft.AspNetCore.Mvc;

namespace RateLimiter;

/// <summary>
/// Applies rate limiting policy to this Action|Controller
/// </summary>
/// <param name="policyName"></param>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class RateLimiterAttribute(string policyName) : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!await RateLimiter.IsRequestAllowedAsync(context.HttpContext, policyName, context.HttpContext.RequestAborted))
        {
            context.Result = new StatusCodeResult(403);
            return;
        }
        await next();
    }
}