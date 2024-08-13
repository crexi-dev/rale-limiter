using RateLimiter.Configuration;
using RateLimiter.Examples.Rules;
using RateLimiter.Extensions;
using RateLimiter.Policies;
using RateLimiter.Rules;

namespace RateLimiter.Examples.Policies;

public class MyCustomComplexGeoPolicy(IRateLimiterStorage storage, IConfiguration configuration) : RateLimiterPolicy
{
    public const string PolicyName = RateLimiterPolicyNames.MyCustomComplexGeoPolicy;

    private readonly ComplexGeoPolicySettings settings = configuration.GetRateLimiterPolicySettings<ComplexGeoPolicySettings>(PolicyName);

    public override IEnumerable<IRateLimiterRule> GetRuleset(HttpRequest request)
    {
        if (request.Headers.TryGetValue("Location", out var locationHeader))
        {
            yield return locationHeader.FirstOrDefault() switch
            {
                "US" => new FixedWindowRule(TimeSpan.FromSeconds(settings.InSeconds), settings.MaxRequestsCount, storage, GetGroupId(request, settings, PolicyName)),
                "EU" => new SlidingWindowRule(TimeSpan.FromSeconds(settings.MinTimeoutInSeconds), storage, GetGroupId(request, settings, PolicyName)),
                _ => new MyCustomBanEveryoneRule()
            };
        }
        else
        {
            yield return new MyCustomBanEveryoneRule();
        }
    }

    class ComplexGeoPolicySettings : BasePolicySettings
    {
        public int MaxRequestsCount { get; set; } = 20;
        public int InSeconds { get; set; } = 1;
        public int MinTimeoutInSeconds { get; set; } = 5;
    }
}
