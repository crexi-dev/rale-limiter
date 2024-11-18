using Microsoft.Extensions.DependencyInjection;
using Cache.Providers;
using Microsoft.Extensions.Configuration;
using RateLimiter.Services;
using RateLimiter.Interfaces;

namespace Cache.Extensions;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureRateLimiter(this IServiceCollection services)
    {
        services.AddSingleton(typeof(IRateLimiterService), typeof(RateLimiterService));
        return services;
    }
}
