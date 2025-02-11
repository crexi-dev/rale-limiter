using Microsoft.AspNetCore.Http;

using RateLimiter.Abstractions;

namespace RateLimiter.Discriminators
{
    public class GeoBasedDiscriminator : IProvideADiscriminator
    {
        public (bool IsMatch, string MatchValue) GetDiscriminator(HttpContext context, IDefineARateLimitRule rateLimitRule)
        {
            // get the ip address via cache/external source

            // perform a geo lookup on it

            // return the geolocation
            return (false, "US");
        }
    }
}
