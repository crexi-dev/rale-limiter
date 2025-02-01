using System;
using Microsoft.Extensions.Caching.Memory;
using RateLimiter.Model;
using RateLimiter.Rules;

namespace RateLimiter.Strategies
{
    public class RateLimitResource3Service : IRateLimitService
    {
        //In real project the values should be moved to configuration.
        public const string Resource3Name = "resource3";
        public const int MaxRequests = 5;
        public const int Period = 1;
        public const int Interval = 2;


        private IMemoryCache _memoryCache;
        private bool _isAllowed;
        private ClientModel _clientData;
        
        public RateLimitResource3Service(IMemoryCache memoryCache, ClientModel clientData)
        {
            _memoryCache = memoryCache;
            _clientData = clientData;
        }
        public bool IsRequestAllowed()
        {
            var isUSbasedRegion = string.Equals(_clientData.Region, "US", StringComparison.InvariantCultureIgnoreCase);
            var isEUbasedRegion = string.Equals(_clientData.Region, "EU", StringComparison.InvariantCultureIgnoreCase);

            _isAllowed = new RateLimitRuleBuilder(_memoryCache, _clientData,Resource3Name)
                .ApplyRule(isUSbasedRegion, new RequestCountRule(MaxRequests, TimeSpan.FromMinutes(Period)))
                .ApplyRule(isEUbasedRegion, new TimeSinceLastCallRule(TimeSpan.FromSeconds(Interval)))
                .Build()
                .IsAllowed();

            return _isAllowed;
        }
    }
}