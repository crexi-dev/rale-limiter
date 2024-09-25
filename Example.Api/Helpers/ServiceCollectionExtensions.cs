using Example.Api.Attributes;
using Example.Api.Managers;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace Example.Api.Helpers;

public static class ServiceCollectionExtensions
{
    public static void RegisterControllers(this IServiceCollection collection, IRateLimitManager limitManager)
    {
        var controllers = Assembly.GetExecutingAssembly().GetTypes().Where(type => typeof(ControllerBase).IsAssignableFrom(type));
        foreach (var controller in controllers)
        {
            var methods = controller.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                .Where(m => m.GetCustomAttributes(typeof(IRateLimitAttribute), true).Any());
            foreach (var method in methods)
            {
                var fullName = method.DeclaringType.Name;
                limitManager.RegisterResource($"{fullName}.{method.Name}", method);
            }
        }
    }
}