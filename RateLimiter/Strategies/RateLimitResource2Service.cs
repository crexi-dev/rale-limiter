using System;
using Microsoft.Extensions.Caching.Memory;
using RateLimiter.Model;
using RateLimiter.Rules;

namespace RateLimiter.Strategies
{
    public class RateLimitResource2Service : IRateLimitService
    {
        //In real project the values should be moved to configuration.
        public const string Resource2Name = "resource2";
        public const int MaxRequests = 5;
        public const int Period = 2;
        private const int Interval = 200;

        private IMemoryCache _memoryCache;
        private bool _isAllowed;
        private ClientModel _clientData;

        public RateLimitResource2Service(IMemoryCache memoryCache, ClientModel clientData)
        {
            _memoryCache = memoryCache;
            _clientData = clientData;
        }
        public bool IsRequestAllowed()
        {
            _isAllowed = new RateLimitRuleBuilder(_memoryCache, _clientData, Resource2Name)
                .WithTimeSinceLastCallRule(TimeSpan.FromSeconds(Interval))
                .WithRequestCountRule(MaxRequests, TimeSpan.FromMinutes(Period))
                .Build()
                .IsAllowed();

            return _isAllowed;
        }
    }
}