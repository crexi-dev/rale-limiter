using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using RateLimiter.Policies;

namespace RateLimiter.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers default InMemory implementation of storage contract used by defaut policies
    /// </summary>
    public static IServiceCollection UseRateLimiterInMemoryStorage(this IServiceCollection serviceCollection) => serviceCollection.AddSingleton<IRateLimiterStorage, RateLimiterInMemoryStorage>();

    /// <summary>
    /// Registers custom implementation of storage contract used by defaut policies
    /// </summary>
    public static IServiceCollection AddRateLimiterStorage<TStorage>(this IServiceCollection serviceCollection) where TStorage : class, IRateLimiterStorage =>
        serviceCollection.AddSingleton<IRateLimiterStorage, TStorage>();

    /// <summary>
    /// Registers Default Sliding Window policy with a specific Policy Key
    /// </summary>
    public static IServiceCollection AddSlidingWindowRateLimiterPolicy(this IServiceCollection serviceCollection, string policyName) =>
        serviceCollection.AddKeyedSingleton<IRateLimiterPolicy, SlidingWindowPolicy>(policyName, (sp, key) => new SlidingWindowPolicy(sp.GetRequiredService<IConfiguration>(), sp.GetRequiredService<IRateLimiterStorage>(), policyName));
    
    /// <summary>
    /// Registers Default Fixed Window policy with a specific Policy Key
    /// </summary>
    public static IServiceCollection AddFixedWindowRateLimiterPolicy(this IServiceCollection serviceCollection, string policyName) =>
        serviceCollection.AddKeyedSingleton<IRateLimiterPolicy, FixedWindowPolicy>(policyName, (sp, key) => new FixedWindowPolicy(sp.GetRequiredService<IConfiguration>(), sp.GetRequiredService<IRateLimiterStorage>(), policyName));
}