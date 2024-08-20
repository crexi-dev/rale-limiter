using RateLimiter.Options;
using System.Diagnostics.CodeAnalysis;

namespace RateLimiter.Configurations;

[ExcludeFromCodeCoverageAttribute]
public static class OptionConfigs
{
    internal static IServiceCollection AddOptionConfigs(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RateLimiterOptions>(configuration.GetSection(key: RateLimiterOptions.RateLimiter));
        return services;

    }
}