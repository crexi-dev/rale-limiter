using RateLimiter.Abstractions;

namespace RateLimiter.Tests.Api.Middleware
{
    public class GeoTokenDiscriminator : IProvideADiscriminator
    {
        public string GetDiscriminator(HttpContext context)
        {
            // get the token
            return context.Request.Query["apiKey"].FirstOrDefault() ?? string.Empty;
        }
    }
}
