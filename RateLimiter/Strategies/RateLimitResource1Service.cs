using System;
using Microsoft.Extensions.Caching.Memory;
using RateLimiter.Model;
using RateLimiter.Rules;

namespace RateLimiter.Strategies
{
    public class RateLimitResource1Service : IRateLimitService
    {
        //In real project the values should be moved to configuration.
        public const string Resource1Name = "resource1";
        public const int MaxRequests = 3;
        public const int Period = 1;


        private IMemoryCache _memoryCache;
        private bool _isAllowed;
        private ClientModel _clientData;
        
        public RateLimitResource1Service(IMemoryCache memoryCache, ClientModel clientData) 
        {
            _memoryCache = memoryCache;
            _clientData = clientData;
        }

        public bool IsRequestAllowed()
        {
            _isAllowed = new RateLimitRuleBuilder(_memoryCache, _clientData, Resource1Name)
                .WithRequestCountRule(MaxRequests, TimeSpan.FromMinutes(Period))
                .Build()
                .IsAllowed();

            return _isAllowed;
        }
    }
}