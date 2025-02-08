using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RateLimiter.Abstractions;
using RateLimiter.Middleware;

namespace RateLimiter.DependencyInjection;

public static class RateLimiterRegister
{
    public static IServiceCollection AddRateLimiting(this IServiceCollection services)
    {
        // TODO: Need the configuration
        services.AddSingleton<IProvideRateLimitRules, RateLimiterRulesFactory>();
        services.AddSingleton<IRateLimitRequests, RateLimiter>();
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