﻿using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using RateLimiter.Contracts;
using RateLimiter.Enums;
using RateLimiter.Models;
using RateLimiter.RateLimitSettings;

namespace RateLimiter.RateLimitRules;

[UsedImplicitly]
public class TimeSpanSinceLastCallRule(
    IOptionsSnapshot<TimeSpanSinceLastCallRuleSettings> options,
    IRequestsStorage requestsStorage)
    : IRateLimitRule
{
    public RegionType RegionType => RegionType.Eu;

    public RuleType RuleType => RuleType.TimeSpanSinceLastCall;
    
    public bool Validate(Request request)
    {
        var requests = requestsStorage.Get(request.Id)
            .Where(x => x.RegionType == request.RegionType)
            .ToList();
        
        if (requests.Count == 0)
        {
            return AddToStorage(request);
        }
        
        var timeSinceLastRequest = DateTime.UtcNow - requests[^1].DateTime;

        return timeSinceLastRequest > TimeSpan.FromMinutes(options.Value.MinimumIntervalInMinutes) && AddToStorage(request);
    }

    private bool AddToStorage(Request request)
    {
        requestsStorage.Add(request);
        
        return true;
    }
}