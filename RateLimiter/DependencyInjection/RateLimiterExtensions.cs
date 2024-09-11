using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using RateLimiter.Rules;

namespace RateLimiter.DependencyInjection;

public static class RateLimiterExtensions
{
    public static IServiceCollection AddRateLimiterServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IRateLimitRuleRepository, RateLimitRuleRepository>();
        services.AddSingleton<IRateLimiter, RateLimiter>();

        var apiConfigSection = configuration.GetSection(RateLimiterConfig.SectionName);
        var apiConfig = new RateLimiterConfig();
        apiConfigSection.Bind(apiConfig);
        
        new RateLimiterConfigValidator().ValidateAndThrow(apiConfig);
        services.AddOptions<RateLimiterConfig>().Bind(apiConfigSection);
        services
            .TryAddSingleton<IValidateOptions<RateLimiterConfig>,
                RateLimiterConfigValidator>();

        return services;
    }
}