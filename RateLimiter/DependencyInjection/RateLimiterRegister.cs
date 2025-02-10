using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using RateLimiter.Abstractions;
using RateLimiter.Common;
using RateLimiter.Discriminators;
using RateLimiter.Middleware;

namespace RateLimiter.DependencyInjection;

public static class RateLimiterRegister
{
    public static IServiceCollection AddRateLimiting(this IServiceCollection services)
    {
        // TODO: Need the configuration
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<IProvideDiscriminators, DiscriminatorProvider>();
        services.AddSingleton<IProvideRateLimitRules, RateLimiterRulesFactory>();
        services.AddSingleton<IRateLimitRequests, RateLimiter>();
        return services;
    }

    // TODO: Allow consumers to register their own custom discriminators (shows extensibility)
    public static IServiceCollection WithCustomDiscriminator<T>(this IServiceCollection services)
        where T : class, IProvideADiscriminator
    {
        //want
        services.AddKeyedSingleton<IProvideADiscriminator, T>(typeof(T).Name);
        //services.AddKeyedSingleton<IDefineRateLimitRules, RequestPerTimespanRule>("RequestPerTimespanRule");

        //services.AddSingleton<IDefineRateLimitRules, customRule>();
        //services.AddSingleton<IDefineRateLimitRules, typeof(customRule)>();
        return services;
    }

    public static WebApplication UseRateLimiting(this WebApplication app)
    {
        app.UseMiddleware<RateLimiterMiddleware>();
        return app;
    }

    public static RouteHandlerBuilder WithRateLimiting(this RouteHandlerBuilder builder)
    {
        // TODO: Implement
        return builder;
    }
}