using Ardalis.GuardClauses;
using RateLimiter;

namespace TestApi.Middleware
{
    public class RateLimitingMiddleware(
       IRateLimiter rateLimiter,
        ILogger<RateLimitingMiddleware> logger): IMiddleware
    {
        private readonly IRateLimiter _rateLimiter = Guard.Against.Null(rateLimiter);
        private readonly ILogger<RateLimitingMiddleware> _logger = Guard.Against.Null(logger);

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            Guard.Against.Null(context);
            Guard.Against.Null(next);

            // Extract client token and resource path from the request
            var clientToken = context.Request.Headers["Authorization"];
            var resource = context.Request.Path.Value;

            if (string.IsNullOrEmpty(clientToken))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Authorization token is missing or empty.");
                return;
            }

            if (string.IsNullOrEmpty(resource))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Resource identifier is missing or empty.");
                return;
            }

            var clientId = GetClientId(clientToken!);
            try
            {
                if (!await _rateLimiter.IsRequestAllowed(clientId, resource))
                {
                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while applying rate limiting");
                // Handle error gracefully, possibly allow the request or return a different status
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync("An error occurred while processing your request.");
                return;
            }

            // Call the next middleware in the pipeline
            await next(context);
        }

        private string GetClientId(string clientToken)
        {
            //TODO pull ClientId from the authorization token
            return clientToken;
        }
    }
}
