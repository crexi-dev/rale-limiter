using Microsoft.AspNetCore.Http;

using RateLimiter.Config;
using RateLimiter.Discriminators;

namespace RateLimiter.Abstractions
{
    public interface IRateLimitDiscriminator
    {
        RateLimiterConfiguration.DiscriminatorConfiguration Configuration { get; set; }

        DiscriminatorEvaluationResult Evaluate(HttpContext context);
    }
}
