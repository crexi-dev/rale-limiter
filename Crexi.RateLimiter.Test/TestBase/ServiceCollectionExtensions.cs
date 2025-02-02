using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Crexi.RateLimiter.Test.TestBase;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddScopedMock<T>(this IServiceCollection services) where T : class =>
        services
            .AddScoped<Mock<T>>(_ => new Mock<T>())
            .AddScoped<T>(p => p.GetRequiredService<Mock<T>>().Object);
}