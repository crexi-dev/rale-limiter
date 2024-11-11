using RateLimiter.Common.Exceptions;
using RateLimiter.Repository.TrafficRepository;

namespace RateLimiter.Services.Limiters;

public class FixedRequestsPerTimeSpanLimiter(ITrafficRepository trafficRepository) : ILimiter
{
    public virtual async Task Limit(RateLimitingOptions options, RequestContext context)
    {
        if (options.MaxRequests <= 0 || options.TimeSpan == TimeSpan.Zero)
        {
            throw new ArgumentException("Invalid rate limiting configuration: MaxRequests and TimeSpan must be greater than zero.");
        }

        var requestCount = await trafficRepository.CountTraffic(context.Token, context.Resource, options.TimeSpan);

        if (requestCount >= options.MaxRequests)
        {
            throw new RateLimitExceededException(context.Resource, requestCount, options.MaxRequests, options.TimeSpan);
        }
    }
}
