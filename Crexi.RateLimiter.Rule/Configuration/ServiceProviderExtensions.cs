using Crexi.RateLimiter.Rule.Configuration.Sections;
using Crexi.RateLimiter.Rule.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Crexi.RateLimiter.Rule.Configuration;

public static class ServiceProviderExtensions
{
    /// <summary>
    /// Registers rules from the configuration, if any
    /// Should be called after app is started
    /// </summary>
    /// <param name="serviceProvider"></param>
    public static void RegisterStartupRules(this IServiceProvider serviceProvider)
    {
        var ruleConfiguration = serviceProvider.GetRequiredService<IOptions<RateLimitRulesConfiguration>>()?.Value;
        if (ruleConfiguration?.StartupRules is null) return;
        var engine = serviceProvider.GetRequiredService<IRateLimitEngine>();
        engine.AddUpdateRules(ruleConfiguration.StartupRules);
    }
}