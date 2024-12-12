using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RateLimiter.Persistence;
using RateLimiter.RuleApplicators;
using RateLimiter.Rules;

namespace RateLimiter.Middleware;

public static class CustomRateLimiterMiddlewareExtensions
{
    public static IServiceCollection AddCustomRateLimiterRules(this IServiceCollection services)
    {
        services.AddScoped<IApplyARateLimit, TooManyInTimeSpanRateLimitRuleApplicator>();
        services.AddScoped<IApplyARateLimit, TooCloseToLastRequestRateLimitRuleApplicator>();

        // TODO: Change to an implementation that uses RedisCache or some other distributed caching mechanism as the default provider
        services.AddSingleton<IProvideAccessToCachedData, InMemoryCacheRepository>();
        // TODO: Change to an implementation that gets data from a database or other persisted storage
        services.AddSingleton<IProvideAccessToConfigurationData, StaticConfigurationRepository>();

        return services;
    }

    public static IApplicationBuilder UseCustomRateLimiter(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CustomRateLimiterMiddleware>();
    }
}