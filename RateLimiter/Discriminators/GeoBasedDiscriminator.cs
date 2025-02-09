using Microsoft.AspNetCore.Http;

using RateLimiter.Abstractions;

namespace RateLimiter.Discriminators
{
    public class GeoBasedDiscriminator : IProvideADiscriminator
    {
        public string GetDiscriminator(HttpContext context)
        {
            // get the ip address via cache/external source

            // perform a geo lookup on it

            // return the geolocation
            return "US";
        }
    }
}
