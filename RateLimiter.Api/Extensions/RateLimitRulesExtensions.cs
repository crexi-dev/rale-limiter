using RateLimiter.Contracts;

namespace RateLimiter.Api.Extensions;

public static class RateLimitRulesExtensions
{
    public static void AddRateLimitRules(this IServiceCollection services)
    {
        var implementations = typeof(IRateLimitRule).Assembly
            .GetTypes()
            .Where(type => typeof(IRateLimitRule).IsAssignableFrom(type) && type.IsClass)
            .ToList();

        foreach (var implementation in implementations)
        {
            services.AddScoped(typeof(IRateLimitRule), implementation);
        }
    }
}