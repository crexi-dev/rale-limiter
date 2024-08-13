using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using RateLimiter.Configuration;
using RateLimiter.Extensions;
using RateLimiter.Rules;

namespace RateLimiter.Policies;

internal class FixedWindowPolicy(IConfiguration configuration, IRateLimiterStorage storage, string policyName) : RateLimiterPolicy
{
    private readonly IRateLimiterStorage storage = storage;
    private readonly string policyName = policyName;
    private readonly FixedWindowPolicySettings settings = configuration.GetRateLimiterPolicySettings<FixedWindowPolicySettings>(policyName);

    public override IEnumerable<IRateLimiterRule> GetRuleset(HttpRequest request)
    {
        var groupId = GetGroupId(request, settings, policyName);
        yield return new FixedWindowRule(TimeSpan.FromSeconds(settings.InSeconds), settings.MaxRequestsCount, storage, groupId);
    }
}
