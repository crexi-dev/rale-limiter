using Microsoft.AspNetCore.Http;

namespace RateLimiter.Abstractions
{
    public interface IProvideADiscriminator
    {
        string GetDiscriminator(HttpContext context, IDefineARateLimitRule rateLimitRule);
    }
}
