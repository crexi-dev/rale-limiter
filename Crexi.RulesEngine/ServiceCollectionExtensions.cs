using Microsoft.Extensions.DependencyInjection;
using Crexi.Cache.Providers;
using Microsoft.Extensions.Configuration;
using Crexi.RulesService.Interfaces;
using Crexi.RulesService.Services;

namespace Crexi.RulesService;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureRulesService(this IServiceCollection services)
    {
        services.AddSingleton(typeof(IRulesService), typeof(RulesJsonService));
        return services;
    }
}
