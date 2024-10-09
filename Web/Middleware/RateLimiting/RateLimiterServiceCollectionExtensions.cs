namespace Web.Middleware.RateLimiting;

using RateLimiter;
using RateLimiter.Rules;

public static class RateLimiterServiceCollectionExtensions
{
    public static IServiceCollection ConfigureWeatherRateLimiter(this IServiceCollection services)
    {
        services.AddKeyedSingleton<IRateLimiter, WeatherRateLimiter>("Weather");
        return services;
    }

    public static IServiceCollection ConfigureNewsRateLimiter(this IServiceCollection services)
    {
        services.AddKeyedSingleton<IRateLimiter, NewsRateLimiter>("News");
        return services;
    }
}

public class WeatherRateLimiter : IRateLimiter
{
    private readonly RateLimiter _rateLimiter;
    
    public WeatherRateLimiter()
    {
        _rateLimiter = new RateLimiter();

        var weatherRule = new RequestsPerTimeSpanRule(5, TimeSpan.FromMinutes(1));
        _rateLimiter.ConfigureRateLimitRule("Weather", weatherRule);
    }

    public Task<(bool isAllowed, DateTime nextAllowedTime)> IsRequestAllowedAsync(string resourceName, string token)
    {
        return _rateLimiter.IsRequestAllowedAsync(resourceName, token);
    }
}

public class NewsRateLimiter : IRateLimiter
{
    private readonly RateLimiter _rateLimiter;
    
    public NewsRateLimiter()
    {
        _rateLimiter = new RateLimiter();

        var newsRule = new TimeSinceLastCallRule(TimeSpan.FromSeconds(10));
        _rateLimiter.ConfigureRateLimitRule("News", newsRule);
    }

    public Task<(bool isAllowed, DateTime nextAllowedTime)> IsRequestAllowedAsync(string resourceName, string token)
    {
        return _rateLimiter.IsRequestAllowedAsync(resourceName, token);
    }
}
