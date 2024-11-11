
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RateLimiter.Repository.TrafficRepository;

namespace RateLimiter.Repository;

public static class Configure
{
    public static void ConfigureInMemoryRepository(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<ITrafficRepository, InMemoryTrafficRepository>();
    }
}

