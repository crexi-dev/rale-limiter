using RateLimiter.Interfaces;
using RateLimiter.Services;

namespace RateLimiter.Configurations;

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