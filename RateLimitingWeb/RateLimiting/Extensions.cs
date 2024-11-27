using RateLimiter.Components.CountryDataProvider;
using RateLimiter.Components.Repository;
using RateLimiter.Components.RuleService;
using RateLimiter.Components.RuleService.Rules.RuleNRequestPerTimerange;
using RateLimiter.Components.RuleService.Rules.RuleAllow1RequestForMatchingConfiguration;
using RateLimiter.Components.RulesService.Rules.DummyRule;
using RateLimiter.Models;
using RateLimiter.Models.Constants;

namespace RateLimiterWeb.RateLimiting
{
    public static class Extensions
    {
        public static IServiceCollection AddRateLimiter(this IServiceCollection services)
        {
            services
                .AddScoped<IRateLimitingService, RateLimitingService>()
                .AddSingleton<IDataRepository, DataRepository>()
                .AddSingleton<ICountryDataProvider, CountryDataProvider>()

                // rules
                .AddScoped<IRateLimitingRule, RuleNRequestPerTimerange>()
                .AddScoped<IRateLimitingRule, RuleAllow1RequestForMatchingConfiguration>()
                .AddScoped<IRateLimitingRule, DummyRule>()
                ;

            return services;
        }

        public static IServiceCollection AddRateLimiterGlobalGroup(this IServiceCollection services, params Action<RateLimitingConfigurationSet>[] configSets)
        {
            var group = new RateLimitingRuleGroup()
            {
                Name = RateLimitingConstants.GlobalGroupName
            };

            foreach (var configure in configSets)
            {
                RateLimitingConfigurationSet map = new();
                configure(map);
                group.ConfigurationSets.Add(map);
            }

            services.AddScoped(provider => group);

            return services;
        }

        public static IServiceCollection AddRateLimiterGroup(this IServiceCollection services, string groupName, params Action<RateLimitingConfigurationSet>[] configSets)
        {
            var group = new RateLimitingRuleGroup()
            {
                Name = groupName
            };

            foreach (var configure in configSets)
            {
                RateLimitingConfigurationSet map = new();
                configure(map);
                group.ConfigurationSets.Add(map);
            }

            services.AddScoped(provider => group);

            return services;
        }
    }
    
}
