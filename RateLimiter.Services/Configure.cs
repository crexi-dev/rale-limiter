
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RateLimiter.Services.Limiters;
using RateLimiter.Services.RateLimitingService;
using RateLimiter.Services.RequestContextService;

namespace RateLimiter.Services;

public static class Configure
{
    public static void ConfigureRateLimiterServices(this IHostApplicationBuilder builder)
    {
        Repository.Configure.ConfigureInMemoryRepository(builder);
        builder.Services.AddScoped<IRequestContextService, ScopedRequestContextService>();
        builder.Services.AddScoped<IRateLimitingService, RateLimitingService.RateLimitingService>();
        builder.Services.AddScoped<FixedRequestsPerTimeSpanLimiter>();
        builder.Services.AddScoped<TimeSpanSinceLastRequestLimiter>();
    }
}

