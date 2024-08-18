using RateLimiter.Options;

namespace RateLimiter.Configurations;

public static class OptionConfigs
{
    public static IServiceCollection AddOptionConfigs(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RateLimiterOptions>(configuration.GetSection(key: RateLimiterOptions.RateLimiter));
        return services;

    }
}