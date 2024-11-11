using RateLimiter.Common.Exceptions;
using RateLimiter.Repository.TrafficRepository;

namespace RateLimiter.Services.Limiters;

public class TimeSpanSinceLastRequestLimiter(ITrafficRepository trafficRepository) : ILimiter
{
    public virtual async Task Limit(RateLimitingOptions options, RequestContext context)
    {
        if (options.TimeSpan == TimeSpan.Zero)
        {
            throw new ArgumentException("TimeSpan must be greater than zero.");
        }

        Traffic? lastTraffic = await trafficRepository.GetLatestTraffic(context.Token, context.Resource);

        if (lastTraffic != null)
        {
            TimeSpan timeSinceLastRequest = DateTime.UtcNow - lastTraffic.Time;

            if (timeSinceLastRequest < options.TimeSpan)
            {
                throw new RateLimitExceededException(context.Resource, timeSinceLastRequest, options.MaxRequests, options.TimeSpan);
            }
        }
    }
}