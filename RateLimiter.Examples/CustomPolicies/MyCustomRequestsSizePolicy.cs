using RateLimiter.Configuration;
using RateLimiter.Examples.Rules;
using RateLimiter.Extensions;
using RateLimiter.Policies;

namespace RateLimiter.Examples.Policies;

public class MyCustomRequestsSizePolicy(IRateLimiterStorage storage, IConfiguration configuration) : RateLimiterPolicy
{
    public const string PolicyName = RateLimiterPolicyNames.MyCustomRequestsSizePolicy;

    private readonly RequestSizePolicySettings settings = configuration.GetRateLimiterPolicySettings<RequestSizePolicySettings>(PolicyName);

    public override IEnumerable<IRateLimiterRule> GetRuleset(HttpRequest request)
    {
        yield return new MyCustomRequestSizeRule(TimeSpan.FromSeconds(settings.InSeconds), settings.MaxBytes, storage, GetGroupId(request, settings, PolicyName));
    }

    class RequestSizePolicySettings : BasePolicySettings
    {
        public int InSeconds { get; set; }
        public int MaxBytes { get; set; }
    }

}




