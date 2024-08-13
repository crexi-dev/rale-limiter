using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using RateLimiter.Configuration;
using RateLimiter.Extensions;
using RateLimiter.Rules;

namespace RateLimiter.Policies;

internal class SlidingWindowPolicy(IConfiguration configuration, IRateLimiterStorage storage, string policyName) : RateLimiterPolicy
{
    private readonly IRateLimiterStorage storage = storage;
    private readonly string policyName = policyName;
    private readonly SlidingWindowPolicySettings settings = configuration.GetRateLimiterPolicySettings<SlidingWindowPolicySettings>(policyName);

    public override IEnumerable<IRateLimiterRule> GetRuleset(HttpRequest request)
    {
        var groupId = GetGroupId(request, settings, policyName);
        yield return new SlidingWindowRule(TimeSpan.FromSeconds(settings.MinTimeoutInSeconds), storage, groupId);
    }
}
