using RateLimiter.Interfaces;
using RateLimiter.Services;
using System.Diagnostics.CodeAnalysis;

namespace RateLimiter.Configurations;

[ExcludeFromCodeCoverageAttribute]
public static class RateLimiterServiceConfigs
{
    public static IServiceCollection AddRateLimiter(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();
        services.AddScoped<IMemoryCacheService, MemoryCacheService>();
        services.AddOptionConfigs(configuration);
        services.AddRateLimiterFactoryServices();

        return services;
    }
}