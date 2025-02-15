using Microsoft.AspNetCore.Http;

using RateLimiter.Abstractions;

using static RateLimiter.Config.RateLimiterConfiguration;

namespace RateLimiter.Discriminators
{
    public class IpAddressDiscriminator(DiscriminatorConfiguration configuration) : IRateLimitDiscriminator
    {
        public DiscriminatorConfiguration Configuration { get; set; }

        public DiscriminatorEvaluationResult Evaluate(HttpContext context)
        {
            // TODO: This is likely incorrect. Cannot test b/c shows "localhost"
            var ipAddress = context.Request.Headers.Host.ToString();

            if (string.IsNullOrEmpty(configuration.DiscriminatorMatch) ||
                configuration.DiscriminatorMatch == "*")
                return new DiscriminatorEvaluationResult(configuration.Name)
                {
                    IsMatch = true,
                    MatchValue = ipAddress
                };

            return configuration.DiscriminatorMatch == ipAddress ?
                new DiscriminatorEvaluationResult(configuration.Name)
                {
                    IsMatch = true,
                    MatchValue = ipAddress
                } :
                new DiscriminatorEvaluationResult(configuration.Name)
                {
                    IsMatch = false,
                    MatchValue = ipAddress
                };
        }
    }
}
