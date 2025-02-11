using Microsoft.AspNetCore.Http;

using RateLimiter.Abstractions;

namespace RateLimiter.Discriminators
{
    public class RequestHeaderDiscriminator : IProvideADiscriminator
    {
        public (bool IsMatch, string MatchValue) GetDiscriminator(HttpContext context, IDefineARateLimitRule rateLimitRule)
        {
            if (string.IsNullOrEmpty(rateLimitRule.DiscriminatorKey))
            {
                // likely should log and throw
                return (false, string.Empty);
            }

            if (!context.Request.Headers.TryGetValue(rateLimitRule.DiscriminatorKey, out var value))
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
}
