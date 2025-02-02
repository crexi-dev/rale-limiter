using Crexi.RateLimiter.Rule.Configuration.Sections;
using Crexi.RateLimiter.Rule.Execution;
using Crexi.RateLimiter.Rule.Model;
using Crexi.RateLimiter.Rule.Validation;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Crexi.RateLimiter.Rule.Configuration;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the local objects to run.
    /// NOTE: Has an unregistered dependencies on TimeProvider and MemoryCache.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection ConfigureRateLimiterRules(this IServiceCollection services,
        IConfiguration configuration)
    {
        return services
            .Configure<RateLimiterConfiguration>(configuration.GetSection(key: "RateLimiter"))
            .Configure<RateLimitRulesConfiguration>(configuration.GetSection(key: "RateLimiterRules"))
            .AddScoped<IValidator<RateLimitRule>, RateLimitRuleValidator>()
            .AddScoped<IRuleEvaluationLogic, RuleEvaluationLogic>()
            .AddScoped<IRateLimitEngine, RateLimitEngine>();
    }
}