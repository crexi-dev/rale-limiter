using Microsoft.Extensions.DependencyInjection;
using Cache.Providers;
using Microsoft.Extensions.Configuration;
using Utilities.Cache.Managers;
using Microsoft.Extensions.Caching.Memory;

namespace Cache.Extensions;
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
