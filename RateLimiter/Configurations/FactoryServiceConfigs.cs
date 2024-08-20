using RateLimiter.Enums;
using RateLimiter.Interfaces;
using RateLimiter.Services;
using System.Diagnostics.CodeAnalysis;

namespace RateLimiter.Configurations
{
    [ExcludeFromCodeCoverageAttribute]
    public static class FactoryServiceConfigs
    {
        internal static IServiceCollection AddRateLimiterFactoryServices(this IServiceCollection services)
        {
            services.AddScoped<RateLimitRuleAService>();
            services.AddScoped<RateLimitRuleBService>();
            services.AddScoped<RateLimitRuleCService>();

            services.AddTransient<Func<RateLimitRules, IRateLimitRule>>(serviceProvider => key =>
            {
                return key switch
                {
                    RateLimitRules.RuleA => serviceProvider.GetRequiredService<RateLimitRuleAService>(),
                    RateLimitRules.RuleB => serviceProvider.GetRequiredService<RateLimitRuleBService>(),
                    RateLimitRules.RuleC => serviceProvider.GetRequiredService<RateLimitRuleCService>(),
                    _ => throw new NotSupportedException($"Service with key '{key}' not found.")
                };
            });

            return services;

        }
    }
}
