using Microsoft.Extensions.DependencyInjection;
using Cache.Providers;
using Microsoft.Extensions.Configuration;
using RulesService.Interfaces;
using RulesService.Services;

namespace RulesService;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureRulesService(this IServiceCollection services)
    {
        services.AddSingleton(typeof(IRulesService), typeof(RulesJsonService));
        return services;
    }
}
