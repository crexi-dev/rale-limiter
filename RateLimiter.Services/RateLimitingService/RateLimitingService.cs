using RateLimiter.Repository.TrafficRepository;
using RateLimiter.Services.Limiters;
using RateLimiter.Services.RequestContextService;

namespace RateLimiter.Services.RateLimitingService;

public class RateLimitingService(
    IRequestContextService contextService,
    ITrafficRepository trafficRepository,
    FixedRequestsPerTimeSpanLimiter fixedRequestsLimiter,
    TimeSpanSinceLastRequestLimiter timeSinceLastRequestLimiter
) : IRateLimitingService
{
    #region IRateLimitingService
    public async Task HandleRequest()
    {
        RequestContext context = await contextService.GetRequestContext();

        foreach (RateLimitingOptions option in context.Options)
        {
            await HandleOption(option, context);
        }

        await trafficRepository.RecordTraffic(new Traffic(context.Resource, context.Token));
    }
    #endregion IRateLimitingService

    #region Logic
    private async Task HandleOption(RateLimitingOptions option, RequestContext context)
    {
        if (OptionDoesNotApplyToOriginCountry(option, context))
        {
            return;
        }

        if (option.Method.HasFlag(RateLimitingMethod.RequestsPerTimespan))
        {
            await fixedRequestsLimiter.Limit(option, context);
        }

        if (option.Method.HasFlag(RateLimitingMethod.TimeSpanSinceLastRequest))
        {
            await timeSinceLastRequestLimiter.Limit(option, context);
        }

        return;
    }

    #endregion Logic

    #region Utility
    private static bool OptionDoesNotApplyToOriginCountry(RateLimitingOptions option, RequestContext context)
    {
        return option.ApplicableCountryCodes != null && context.OriginIsoCountryCode != null && !option.ApplicableCountryCodes.Contains(context.OriginIsoCountryCode);
    }
    #endregion 
}
