namespace RateLimiter.Examples;
public class PostRateLimiterMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Method == HttpMethods.Post)
        {
            if (!await RateLimiter.IsRequestAllowedAsync(context, RateLimiterPolicyNames.DefaultAllPostFixedWindowPolicy, context.RequestAborted))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }
        }
        await next(context);

    }
}

