using System;
using Microsoft.Extensions.Logging;
using RateLimiter.Contracts;
using RateLimiter.Infrastructure;
using RateLimiter.Processors;

namespace RateLimiter.Attributes;

public class RateLimiterAttribute : BaseRulesProcessingAttribute<AllowRequestResult, RequestDetails>
{
    
    public RateLimiterAttribute(ICacheRepository<string, DateTime, BlockedClientRecord> blockedSenders, ICacheRepository<string, RequestDetails, CachedRequestsRecord> requestCache, ILogger<BaseRulesProcessingAttribute<AllowRequestResult,RequestDetails>> logger, IProcessorFactory processorFactory, IContextExtender contextExtender) : base(blockedSenders, requestCache, logger, processorFactory, contextExtender)
    {
        DefaultAttributeSettings = new RateLimiterDefaults();
        SetDefaultValues();
    }
}