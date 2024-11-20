using Microsoft.Extensions.DependencyInjection;
using Cache.Providers;

namespace Cache.Extensions;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureCache(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddSingleton(typeof(ICacheProvider), typeof(InMemoryCacheProvider));

        return services;
    }
}
