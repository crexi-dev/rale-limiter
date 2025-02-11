using RateLimiter.Abstractions;
using RateLimiter.Discriminators;

namespace RateLimiter.Tests.Api.Middleware.RateLimiting;

public class GeoTokenDiscriminator : IProvideADiscriminator
{
    /// <summary>
    /// This functionality could have been obtained using <see cref="RequestHeaderDiscriminator"/>, but showing extensibility
    /// </summary>
    /// <param name="context"></param>
    /// <param name="rateLimitRule"></param>
    /// <returns></returns>
    public (bool IsMatch, string MatchValue) GetDiscriminator(HttpContext context, IDefineARateLimitRule rateLimitRule)
    {
        if (!context.Request.Headers.TryGetValue("x-crexi-token", out var value))
        {
            return (false, string.Empty);
        }

        if (string.IsNullOrEmpty(rateLimitRule.DiscriminatorMatch) ||
            rateLimitRule.DiscriminatorMatch == "*")
            return (true, value.ToString());

        return rateLimitRule.DiscriminatorMatch == value.ToString() ?
            (true, value.ToString()) :
            (false, value.ToString());
    }
}