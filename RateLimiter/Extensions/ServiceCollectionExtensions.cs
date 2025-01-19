using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RateLimiter.Configuration;
using RateLimiter.Middleware;
using RateLimiter.Providers;
using RateLimiter.Rules;
using RateLimiter.Store;
using System;

namespace RateLimiter.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRateLimiter(this IServiceCollection services, Action<IRateLimiterConfiguration> configure)
        {
            ArgumentNullException.ThrowIfNull(nameof(services));
            ArgumentNullException.ThrowIfNull(nameof(configure));

            var config = new RateLimiterConfiguration(new InMemoryDataStore(), new DateTimeProvider());

            services.AddSingleton<IRateLimiter, RateLimiter>(provider => new RateLimiter(config));
            
            configure.Invoke(config);
            return services;
        }

        public static IApplicationBuilder UseRateLimiter(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseMiddleware<RateLimiterMiddleware>();
        }
    }
}
