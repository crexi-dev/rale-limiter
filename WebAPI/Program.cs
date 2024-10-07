using Microsoft.Extensions.Caching.Memory;
using RateLimiter.Storage;
using RateLimiter.Interfaces;
using RateLimiter.Rules;
using RateLimiter;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();

builder.Services.AddSingleton<IRateLimiterStorage>(provider =>
{
    var cache = provider.GetRequiredService<IMemoryCache>();
    return new RateLimiterLocalStorage(cache);
});

builder.Services.AddSingleton(provider =>
{
    var rateLimiterStorage = provider.GetRequiredService<IRateLimiterStorage>();
    return new RequestRateLimiterRule(rateLimiterStorage, 2, TimeSpan.FromSeconds(5)); //2 requests every 5 seconds 
});

builder.Services.AddSingleton(provider =>
{
    var rateLimiterStorage = provider.GetRequiredService<IRateLimiterStorage>();
    return new TimeRateLimiterRule(rateLimiterStorage, TimeSpan.FromSeconds(2)); //1 request every 2 seconds
});

builder.Services.AddScoped(provider =>
{
    var usLimiter = provider.GetService<RequestRateLimiterRule>();
    var euLimiter = provider.GetService<TimeRateLimiterRule>();
    return new RegionBasedRateLimiterDelegator(usLimiter, euLimiter);
});

builder.Services.AddControllers(options =>
{
    options.Filters.AddService<RegionBasedRateLimiterDelegator>();
});

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

app.Run();