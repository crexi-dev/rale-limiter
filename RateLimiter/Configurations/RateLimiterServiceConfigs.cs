using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RateLimiter.Configurations;

public static class RateLimiterServiceConfigs
{
    public static IServiceCollection AddRateLimiter(this IServiceCollection services,IConfiguration configuration)
    {
        services.AddMemoryCache();
        services.AddOptionConfigs(configuration);
        services.AddRateLimiterFactoryServices();

        return services;
    }
}