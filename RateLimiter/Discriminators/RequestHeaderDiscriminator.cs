using Microsoft.AspNetCore.Http;

using RateLimiter.Abstractions;

using static RateLimiter.Config.RateLimiterConfiguration;

namespace RateLimiter.Discriminators
{
    public class RequestHeaderDiscriminator(DiscriminatorConfiguration configuration) : IRateLimitDiscriminator
    {
        public DiscriminatorConfiguration Configuration { get; set; }

        public DiscriminatorEvaluationResult Evaluate(HttpContext context)
        {
            if (string.IsNullOrEmpty(configuration.DiscriminatorKey))
            {
                // likely should log and throw
                return new DiscriminatorEvaluationResult(configuration.Name);
            }

            if (!context.Request.Headers.TryGetValue(configuration.DiscriminatorKey, out var value))
            {
                return new DiscriminatorEvaluationResult(configuration.Name);
            }

            if (string.IsNullOrEmpty(configuration.DiscriminatorMatch) ||
                configuration.DiscriminatorMatch == "*")
                return new DiscriminatorEvaluationResult(configuration.Name)
                {
                    IsMatch = true,
                    MatchValue = value.ToString()
                };

            return configuration.DiscriminatorMatch == value.ToString() ?
                new DiscriminatorEvaluationResult(configuration.Name)
                {
                    IsMatch = true,
                    MatchValue = value.ToString()
                } :
                new DiscriminatorEvaluationResult(configuration.Name)
                {
                    IsMatch = false,
                    MatchValue = value.ToString()
                };
        }
    }
}
