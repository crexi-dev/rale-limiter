using RateLimiter.Services;

namespace RateLimiter.Middleware
{
    public class ApiKeyMiddleware(RequestDelegate next)
    {
        /*
            In a real app this would interact with an actual authorization service to validate the API key on the request.
            Hardcoding some API keys here for convenience.
            You can use these API keys to interact with the rate limiter.
            For example:
                curl -k -X 'GET' 'https://localhost:7219/WeatherForecast' -H 'accept: text/plain' -H 'x-api-key: fd546133b56ccdd65a31b86a2b88dd9c'
        */

        private readonly HashSet<string> _europeanApiKeys =
            [
                "fd546133b56ccdd65a31b86a2b88dd9c",
                "49d3ad978e01a8fd57849bd56b66ae8d",
                "49705cbb56ee596e03765cf7815f9858"
            ];

        private readonly HashSet<string> _northAmericanApiKeys =
            [
                "fd546133b56ccdd65a31b86a2b88dd9c",
                "476093bdee0180ac729115221a45a2a1",
                "981729af12bac6b5e8e982aae064b96a"
            ];

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Headers.TryGetValue("x-api-key", out var apiKeyValue) || string.IsNullOrWhiteSpace(apiKeyValue))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("API key is missing");
                return;
            }

            string apiKey = apiKeyValue.ToString();

            bool IsRegionMember(string apiKey, HashSet<string> regionApiKeys) =>
                regionApiKeys.Contains(apiKey);

            bool IsUnknownApiKey(string apiKey, IEnumerable<HashSet<string>> apiKeyCollections) =>
                !apiKeyCollections.Any(collection => IsRegionMember(apiKey, collection));

            if (IsUnknownApiKey(apiKey, [_europeanApiKeys, _northAmericanApiKeys]))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Invalid API key");
                return;
            }

            var applicablePolicies = new HashSet<RateLimiterPolicyEnum>();
            context.Items["ApplicablePolicies"] = applicablePolicies;

            if (IsRegionMember(apiKey, _europeanApiKeys))
            {
                applicablePolicies.Add(RateLimiterPolicyEnum.European);
            }

            if (IsRegionMember(apiKey, _northAmericanApiKeys))
            {
                applicablePolicies.Add(RateLimiterPolicyEnum.NorthAmerican);
            }

            await next(context);
        }
    }

    public static class ApiKeyMiddlewareExtensions
    {
        public static IApplicationBuilder UseApiKeyMiddleware(this IApplicationBuilder builder) =>
            builder.UseMiddleware<ApiKeyMiddleware>();
    }
}
