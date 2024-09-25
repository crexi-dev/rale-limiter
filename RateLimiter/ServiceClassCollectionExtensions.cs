using System;
using RateLimiter.Contracts;
using RateLimiter.Infrastructure;
using RateLimiter.Processors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RateLimiter.Contracts;

namespace RateLimiter;

public static class ServiceCollectionExtensions
{
    public static void ConfigureServices(this IServiceCollection services)
    {
        
        services.AddSingleton<ILogger<BlockedRequestsRepository>>(sp => 
            sp.GetRequiredService<ILoggerFactory>().CreateLogger<BlockedRequestsRepository>());
        services.AddSingleton<ICacheRepository<string, DateTime, BlockedClientRecord>, BlockedRequestsRepository>();
        services.AddSingleton<ICacheRepository<string, RequestDetails, CachedRequestsRecord>, CachedRequestsRepository>();
        services.AddScoped<ProcessorFactory>();
        services.AddScoped<EngineFactory>();
        services.AddScoped(typeof(IContextExtender), typeof(ContextExtensions));
        services.AddScoped(typeof(IProcessorFactory), typeof(ProcessorFactory));
    }
}