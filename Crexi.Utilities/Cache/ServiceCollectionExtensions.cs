using Microsoft.Extensions.DependencyInjection;
using Crexi.Cache.Providers;
using Microsoft.Extensions.Configuration;
using Crexi.Utilities.Cache.Managers;
using Microsoft.Extensions.Caching.Memory;

namespace Crexi.Cache.Extensions;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureCache(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddSingleton(typeof(ICacheManager), typeof(CacheManager));
        services.AddSingleton(typeof(ICacheProvider), typeof(InMemoryCacheProvider));

        return services;
    }
}
