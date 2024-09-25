using Example.Api.Managers;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Globalization;
using System.Reflection;
using Example.Api.Attributes;

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
        if (!context.Request.Headers.ContainsKey("UserName") || !context.Request.Headers.ContainsKey("Origin"))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("No username or origin is provided");
            return;
        }

        var userName = context.Request.Headers["UserName"].ToString();
        var origin = context.Request.Headers["Origin"].ToString();


        var endpoint = context.GetEndpoint();

        var actionDescriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();

        if (actionDescriptor != null)
        {
            var methodInfo = actionDescriptor.MethodInfo;

            var result = limitManager.CanPerformRequest($"{methodInfo.DeclaringType.Name}.{methodInfo.Name}", new UserToken(userName, origin));
            if (result == false)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Rate limit exceeded. Please try again later");
                return;
            }
        }

        await _next(context);
    }
}