using Crexi.RateLimiter.Logic;
using Crexi.RateLimiter.Models;

namespace Crexi.RateLimiterSampleApi.Middleware;

/// <summary>
/// Sample self contained middleware class to determine how to use the rate limit engine
/// This does not use DI, which would require wrapper classes
/// </summary>
/// <param name="next"></param>
/// <param name="logger"></param>
public class RateLimitMiddleware(RequestDelegate next, ILogger<RateLimitMiddleware> logger)
{
    private static RateLimitEngine _rateLimitEngine = new();
    private static List<RateLimitPolicy> _rateLimitPolicies = new();
    private static ClientRequestTracker _clientRequestTracker = new();
    
    // HARDCODED FOR DEMONSTRATION PURPOSES ONLY
    private const long RATE_LIMIT_MINUTES = 60;
    private const long RATE_LIMIT_CALL_COUNT = 5;
    
    static RateLimitMiddleware()
    {
        var rateLimitPolicy = new RateLimitPolicy
        {
            PolicyName = $"{nameof(RateLimiterSampleApi)}.{nameof(RateLimitMiddleware)}",
            PolicyType = PolicyType.SlidingWindow,
            Limit = RATE_LIMIT_CALL_COUNT,
            TimeSpanWindow = TimeSpan.FromMinutes(RATE_LIMIT_MINUTES),
            ApplyClientTagFilter = true
        };
        
        // California (US-CA) users with PREMIUM subscription have double the limit 
        rateLimitPolicy.ClientFilterGroups.Add(
            new ClientFilterGroup
            {
                LimitOverride = 2 * RATE_LIMIT_CALL_COUNT,
                ClientFilters = new ()
                {
                    new ClientFilter
                    {
                        PropertyName = nameof(ClientRequest.RegionCountryCode),
                        TargetValue = "US-CA",
                        HasTargetValue = true
                    },
                    new ClientFilter
                    {
                        PropertyName = nameof(ClientRequest.SubscriptionLevel),
                        TargetValue = "PREMIUM",
                        HasTargetValue = true
                    }
                }
            });
        
        _rateLimitPolicies.Add(rateLimitPolicy);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var request = GetClientRequestFromHeaders(context);
        
        if (request.ClientId != string.Empty)
        {
            var limitCheckResult = _rateLimitEngine.CheckClientRequest(
                request,
                _clientRequestTracker,
                _rateLimitPolicies,
                logger);

            if (limitCheckResult.IsAllowed == false)
            {
                logger.LogWarning($"Client {request.ClientId} exceeded rate limit policies");
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.Response.WriteAsync($"Too many requests");
                return;
            }

            _clientRequestTracker.AddRequest(request);
        }
        
        // Progress pipeline
        await next(context);

        // After request is processed
        if (request.ClientId != string.Empty)
        {
            _clientRequestTracker.EndRequest(request.ClientId, request.RequestId);
        }
    }

    private ClientRequest GetClientRequestFromHeaders(HttpContext context)
    {
        var request = new ClientRequest()
        {
            ClientId = string.Empty,
            RegionCountryCode = string.Empty,
            SubscriptionLevel = string.Empty,
            RequestId = Guid.NewGuid().ToString(),
            RequestTime = DateTime.UtcNow,
        };

        #pragma warning disable CS8601 // Possible null reference assignment.
        if (context.Request.Headers.TryGetValue("ClientId", out var clientId))
        {
            request.ClientId = clientId;
        }
        else
        {
            logger.LogWarning("Unable to retrieve clientId from headers");
        }

        if (context.Request.Headers.TryGetValue("RegionCountryCode", out var regionCode))
        {
            request.RegionCountryCode = regionCode;
        }
        else
        {
            logger.LogWarning("Unable to retrieve regionCode from headers");
        }

        if (context.Request.Headers.TryGetValue("SubscriptionLevel", out var subscription))
        {
            request.SubscriptionLevel = subscription;
        }
        else
        {
            logger.LogWarning("Unable to retrieve regionCode from headers");
        }
        #pragma warning restore CS8601 // Possible null reference assignment.

        return request;
    }
}