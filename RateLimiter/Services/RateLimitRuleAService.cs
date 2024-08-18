using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using RateLimiter.Interfaces;
using RateLimiter.Options;
using System;

namespace RateLimiter.Services;

public class RateLimitRuleAService : IRateLimitRule
{
    private readonly IMemoryCache _memoryCache;
    private readonly IOptionsMonitor<RateLimiterOptions> _optionsMonitor;

    public RateLimitRuleAService(IMemoryCache memoryCache,IOptionsMonitor<RateLimiterOptions> optionsMonitor)
    {
        _memoryCache = memoryCache;
        _optionsMonitor = optionsMonitor;
    }
    public bool IsRequestAllowed(string userToken)
    {
        throw new NotImplementedException();
    }
}
