using RateLimiter.Abstractions;

namespace RateLimiter.Tests.Api.Middleware.RateLimiting;

public class GeoTokenDiscriminator : IProvideADiscriminator
{
    public string GetDiscriminator(HttpContext context)
    {
        // get the token
        return context.Request.Headers["x-crexi-token"].FirstOrDefault() ?? string.Empty;
    }
}