using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using RateLimiter.Abstractions;
using RateLimiter.Common;
using RateLimiter.Config;
using RateLimiter.Discriminators;
using RateLimiter.Middleware;
using RateLimiter.Rules.Algorithms;

namespace RateLimiter.DependencyInjection;

public static class RateLimiterRegister
{
    public static IServiceCollection AddRateLimiting(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<IRateLimiterConfigurationValidator, RateLimiterConfigurationValidator>();
        services.AddSingleton<IRateLimitDiscriminatorProvider, DiscriminatorProvider>();
        services.AddSingleton<IRateLimitAlgorithmProvider, AlgorithmProvider>();
        services.AddSingleton<IRateLimiter, RateLimiter>();
        return services;
    }
    
    /// <summary>
    /// Allow consumers to register their own custom discriminators (shows extensibility)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection WithCustomDiscriminator<T>(this IServiceCollection services)
        where T : class, IRateLimitDiscriminator
    {
        services.AddKeyedSingleton<IRateLimitDiscriminator, T>(typeof(T).Name);
        return services;
    }

    public static IServiceCollection WithConfiguration<TRateLimiterConfiguration>(
        this IServiceCollection services,
        IConfigurationSection section) where TRateLimiterConfiguration : RateLimiterConfiguration
    {
        services.Configure<RateLimiterConfiguration>(section);
        return services;
    }

    public static WebApplication UseRateLimiting(this WebApplication app)
    {
        app.UseMiddleware<RateLimiterMiddleware>();
        return app;
    }

    public static RouteHandlerBuilder WithRateLimitingRule(this RouteHandlerBuilder builder, string ruleName)
    {
        // TODO: Implement
        return builder;
    }
}