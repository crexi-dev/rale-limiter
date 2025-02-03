using System.Reflection;
using RateLimiter.Contracts;

namespace RateLimiter.Api.Extensions;

public static class SettingsExtensions
{
    public static void AddSettings(this IServiceCollection services, IConfiguration configuration)
    {
        var settingsTypes = typeof(ISetting).Assembly
            .GetTypes()
            .Where(type => type.IsAssignableTo(typeof(ISetting)) && type.IsClass)
            .ToList();

        foreach (var method in settingsTypes.Select(type => typeof(SettingsExtensions)
                     .GetMethod(nameof(ConfigureSettings), BindingFlags.Static | BindingFlags.Public)?
                     .MakeGenericMethod(type)))
        {
            method?.Invoke(null, [services, configuration]);
        }
    }
    
    public static void ConfigureSettings<TSetting>(this IServiceCollection services, IConfiguration configuration)
        where TSetting : class, ISetting
    {
        var section = configuration.GetSection(typeof(TSetting).Name);
        services.Configure<TSetting>(section);
        services.AddOptions<TSetting>()
            .Bind(section)
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }
}