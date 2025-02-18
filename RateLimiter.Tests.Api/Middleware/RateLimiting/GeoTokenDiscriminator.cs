using RateLimiter.Abstractions;
using RateLimiter.Discriminators;

using static RateLimiter.Config.RateLimiterConfiguration;

namespace RateLimiter.Tests.Api.Middleware.RateLimiting;

public class GeoTokenDiscriminator : IRateLimitDiscriminator
{
    public DiscriminatorConfiguration Configuration { get; set; }

    /// <summary>
    /// This functionality could have been obtained using <see cref="RequestHeaderDiscriminator"/>, but showing extensibility
    /// </summary>
    /// <param name="context"></param>
    /// <param name="rateLimitRule"></param>
    /// <returns></returns>
    public DiscriminatorEvaluationResult Evaluate(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue("x-crexi-token", out var value))
        {
            return new DiscriminatorEvaluationResult(Configuration.Name)
            {
                IsMatch = false
            };
        }

        return value.ToString().StartsWith("US") ?
            new DiscriminatorEvaluationResult(Configuration.Name)
            {
                IsMatch = true,
                MatchValue = value.ToString(),
                AlgorithmName = Configuration.AlgorithmNames[0]
            } :
            new DiscriminatorEvaluationResult(Configuration.Name)
            {
                IsMatch = true,
                MatchValue = value.ToString(),
                AlgorithmName = Configuration.AlgorithmNames[1]
            };
    }
}