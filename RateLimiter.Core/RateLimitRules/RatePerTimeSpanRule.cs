using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using RateLimiter.Contracts;
using RateLimiter.Enums;
using RateLimiter.Models;
using RateLimiter.RateLimitSettings;

namespace RateLimiter.RateLimitRules;

[UsedImplicitly]
public class RatePerTimeSpanRule(
    IOptionsSnapshot<RatePerTimeSpanRuleSettings> options,
    IRequestsStorage requestsStorage)
    : IRateLimitRule
{
    private static RegionType RegionType => RegionType.Us;
    
    public RuleType RuleType => RuleType.RequestPerTimeSpan;
    
    public bool Validate(Request request)
    {
        if (request.RegionType != RegionType)
        {
            return true;
        }
        
        requestsStorage.RemoveOldRequests(
            request.Id,
            request.RegionType,
            TimeSpan.FromMinutes(options.Value.IntervalInMinutes));
        
        var requests = requestsStorage.Get(request.Id);

        if (requests.Count(x => x.RegionType == RegionType) == options.Value.RequestsCount)
        {
            return false;
        }

        requestsStorage.Add(request);

        return true;
    }
}