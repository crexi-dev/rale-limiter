using RateLimiter.Services;

namespace RateLimiter.Middleware
{
    public class RateLimiterMiddleware(RequestDelegate next, ClientBehaviorCache clientBehaviorCache, IConfiguration configuration)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            context.Request.Headers.TryGetValue("x-api-key", out var apiKey);

            var requestTime = DateTime.UtcNow;

            // Non-null api key and applicable policy instances guaranteed by API Key middleware.
            clientBehaviorCache.Add(apiKey!, requestTime);
            var applicablePolicies = context.Items["ApplicablePolicies"] as HashSet<RateLimiterPolicyEnum>;
            bool isEuropean = applicablePolicies!.Contains(RateLimiterPolicyEnum.European);
            bool isNorthAmerican = applicablePolicies!.Contains(RateLimiterPolicyEnum.NorthAmerican);

            if (isEuropean)
            {
                var europeanPolicy = new EuropeanRateLimiterPolicy(clientBehaviorCache, configuration);

                if (europeanPolicy.IsApplicable(apiKey!, requestTime))
                {
                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    await context.Response.WriteAsync("Too many requests per European policy");
                    return;
                }
            }

            if (isNorthAmerican)
            {
                var northAmericanPolicy = new NorthAmericanRateLimiterPolicy(clientBehaviorCache, configuration);

                if (northAmericanPolicy.IsApplicable(apiKey!, requestTime))
                {
                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    await context.Response.WriteAsync("Too many requests per North American policy");
                    return;
                }
            }

            await next(context);
        }
    }

    public static class RateLimiterMiddlewareExtensions
    {
        public static IApplicationBuilder UseRateLimiterMiddleware(this IApplicationBuilder builder) =>
            builder.UseMiddleware<RateLimiterMiddleware>();
    }
}
