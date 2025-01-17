using Microsoft.Extensions.DependencyInjection;
using RateLimitingLibrary.Config;
using RateLimitingLibrary.Core.Interfaces;
using RateLimitingLibrary.Core.Services;
using RateLimitingLibrary.Rules;
using System;

namespace RateLimitingApp
{
    public class Startup
    {
        public static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Register Rule Configurations
            services.AddSingleton(new RuleConfigurations
            {
                ResourceRules = new()
                {
                    { "ResourceA", new() { "FixedWindowRule" } },
                    { "ResourceB", new() { "SlidingWindowRule", "CustomRegionRule" } }
                }
            });

            // Register Rate Limit Rules
            services.AddSingleton<IRateLimitRule, FixedWindowRule>(provider => new FixedWindowRule(5, TimeSpan.FromMinutes(1)));
            services.AddSingleton<IRateLimitRule, SlidingWindowRule>(provider => new SlidingWindowRule(10, TimeSpan.FromMinutes(5)));
            services.AddSingleton<IRateLimitRule, CustomRegionRule>();

            // Register Rate Limiter Service
            services.AddSingleton<IRateLimiter, RateLimiter>();

            return services.BuildServiceProvider();
        }
    }
}