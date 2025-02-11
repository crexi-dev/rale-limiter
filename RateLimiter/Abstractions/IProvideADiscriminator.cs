using Microsoft.AspNetCore.Http;

namespace RateLimiter.Abstractions
{
    public interface IProvideADiscriminator
    {
        (bool IsMatch, string MatchValue) GetDiscriminator(HttpContext context, IDefineARateLimitRule rateLimitRule);
    }
}
