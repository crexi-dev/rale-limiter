using Microsoft.AspNetCore.Http;

using RateLimiter.Abstractions;

namespace RateLimiter.Discriminators
{
    public class IpAddressDiscriminator : IProvideADiscriminator
    {
        public string GetDiscriminator(HttpContext context, IDefineARateLimitRule rateLimitRule)
        {
            // TODO: This is likely incorrect. Cannot test b/c shows "localhost"
            return context.Request.Headers.Host.ToString();
        }
    }
}
