using Microsoft.Extensions.Hosting;

namespace RateLimiter;

public static class Configure
{
    public static void ConfigureRateLimiter(this IHostApplicationBuilder builder)
    {
        Services.Configure.ConfigureRateLimiterServices(builder);
    }
}
