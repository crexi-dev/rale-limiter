using Microsoft.AspNetCore.Http;

using RateLimiter.Abstractions;

using static RateLimiter.Config.RateLimiterConfiguration;

namespace RateLimiter.Discriminators
{
    public class GeoBasedDiscriminator(DiscriminatorConfiguration configuration) : IRateLimitDiscriminator
    {
        public DiscriminatorConfiguration Configuration { get; set; }

        public DiscriminatorEvaluationResult Evaluate(HttpContext context)
        {
            // get the ip address via cache/external source

            // perform a geo lookup on it

            // return the geolocation
            return new DiscriminatorEvaluationResult(configuration.Name)
            {
                IsMatch = false,
                MatchValue = "US"
            };
        }
    }
}
