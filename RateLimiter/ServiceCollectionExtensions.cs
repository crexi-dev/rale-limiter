using Microsoft.Extensions.DependencyInjection;
using Crexi.Cache.Providers;
using Microsoft.Extensions.Configuration;
using RateLimiter.Services;
using RateLimiter.Interfaces;

namespace Crexi.Cache.Extensions;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureRateLimiter(this IServiceCollection services)
    {
        services.AddSingleton(typeof(IRateLimiterService), typeof(RateLimiterService));
        return services;
    }
}
