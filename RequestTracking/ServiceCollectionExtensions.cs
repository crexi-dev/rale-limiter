using Microsoft.Extensions.DependencyInjection;
using RequestTracking.Interfaces;
using RequestTracking.Services;

namespace RequestTracking;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureRequestTracking(this IServiceCollection services)
    {
        services.AddSingleton(typeof(ITrackingStorageProvider), typeof(CacheTrackingStorageProvider));
        services.AddSingleton(typeof(IRequestTrackingService), typeof(RequestTrackingService));
        return services;
    }
}
