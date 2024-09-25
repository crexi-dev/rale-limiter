using Microsoft.AspNetCore.Mvc.Controllers;
using RateLimiter;
using RateLimiter.Attributes;
using System.Globalization;
using System.Reflection;

namespace Example.Api.Middleware;

public class RequestRateMiddleware
{
    private readonly RequestDelegate _next;

    public RequestRateMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IRateLimitManager limitManager)
    {
        var endpoint = context.GetEndpoint();

        var actionDescriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();

        if (actionDescriptor != null)
        {
            var methodInfo = actionDescriptor.MethodInfo;

            var attributes = methodInfo.GetCustomAttributes().Where(x=> x is IRateLimit);

            foreach (var attribute in attributes)
            {
                var rla = attribute as IRateLimit;
                var result = limitManager.IsRequestAllowed(rla, "");
                if (result == false)
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsync("Rate limit exceeded. Please try again later");
                    return;
                }
            }
        }

        await _next(context);
    }
}