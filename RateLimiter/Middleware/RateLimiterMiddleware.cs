using RateLimiter.Models;
using RateLimiter.Services;
using System.Text.Json;
using System.Text.Json.Serialization;
public class RateLimiterMiddleware
{
    private readonly RequestDelegate _next;
    private readonly RateLimiterManager _rateLimiterManager;

    public RateLimiterMiddleware(RequestDelegate next, RateLimiterManager rateLimiterManager)
    {
        _next = next;
        _rateLimiterManager = rateLimiterManager;
    }

    public async Task Invoke(HttpContext context)
    {
        var path = context.Request.Path.ToString().ToLower();
        if (path.StartsWith("/swagger") || path.StartsWith("/health"))
        {
            await _next(context);
            return;
        }

        string clientId = context.Request.Headers["X-Client-Id"];
        if (string.IsNullOrEmpty(clientId))
        {
            await RespondWithError(
                context,
                StatusCodes.Status400BadRequest,
                RateLimitErrorCode.MissingClientId,
                "Client ID is required in the 'X-Client-Id' header."
            );
            return;
        }

        string resource = context.Request.Path.ToString();
        var rateLimitResult = _rateLimiterManager.IsRequestAllowed(clientId, resource);

        if (!rateLimitResult.IsAllowed)
        {
            var retryAfterSeconds = Math.Ceiling(rateLimitResult.RetryAfter.TotalSeconds);
            context.Response.Headers["Retry-After"] = retryAfterSeconds.ToString();

            await RespondWithError(
                context,
                StatusCodes.Status429TooManyRequests,
                RateLimitErrorCode.RateLimitExceeded,
                "Rate limit exceeded. Try again later.",
                retryAfterSeconds
            );
            return;
        }

        await _next(context);
    }

    private static async Task RespondWithError(
        HttpContext context,
        int statusCode,
        RateLimitErrorCode errorCode,
        string message,
        double? retryAfter = null)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var errorResponse = new ErrorResponse
        {
            Error = new ErrorDetails
            {
                Code = errorCode,
                Message = message,
                RetryAfter = retryAfter
            }
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, options));
    }
}
