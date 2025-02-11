using Microsoft.AspNetCore.Http;

using RateLimiter.Abstractions;

namespace RateLimiter.Discriminators
{
    public class IpAddressDiscriminator : IProvideADiscriminator
    {
        public (bool IsMatch, string MatchValue) GetDiscriminator(HttpContext context, IDefineARateLimitRule rateLimitRule)
        {
            // TODO: This is likely incorrect. Cannot test b/c shows "localhost"
            var ipAddress = context.Request.Headers.Host.ToString();

            if (string.IsNullOrEmpty(rateLimitRule.DiscriminatorMatch) ||
                rateLimitRule.DiscriminatorMatch == "*")
                return (true, ipAddress);

            return rateLimitRule.DiscriminatorMatch == ipAddress ?
                (true, ipAddress) :
                (false, ipAddress);
        }
    }
}
