using Crexi.API.Common.RateLimiter.Interfaces;

public class RateLimiterMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IRateLimiter _rateLimiter;
    private readonly ITokenToClientConverter _tokenToClientConverter;
    private readonly ITokenExtractor _tokenExtractor;

    public RateLimiterMiddleware(RequestDelegate next, IRateLimiter rateLimiter, ITokenToClientConverter tokenToClientConverter, ITokenExtractor tokenExtractor)
    {
        _next = next;
        _rateLimiter = rateLimiter;
        _tokenToClientConverter = tokenToClientConverter;
        _tokenExtractor = tokenExtractor;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var accessToken = _tokenExtractor.ExtractToken(context);

        var client = _tokenToClientConverter.ConvertTokenToClient(accessToken);

        if (client == null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Invalid or malformed access token.");
            return;
        }

        var resource = context.Request.Path.Value;

        if (!_rateLimiter.IsRequestAllowed(client, resource))
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            await context.Response.WriteAsync("Rate limit exceeded.");
            return;
        }

        await _next(context);
    }
}
