using Crexi.API.Common.RateLimiter.Interfaces;
using Crexi.API.Common.RateLimiter.Models;
using System;
using System.Collections.Concurrent;

namespace Crexi.API.Common.RateLimiter.Rules
{
    public class TimeSinceLastCallRule : IRateLimitRule
    {
        private readonly TimeSpan _requiredIntervalBetweenRequestsByClientToResource;
        private readonly ConcurrentDictionary<string, DateTime> _lastCallTimesByClientsToResources;

        public TimeSinceLastCallRule(TimeSpan requiredInterval)
        {
            _requiredIntervalBetweenRequestsByClientToResource = requiredInterval;
            _lastCallTimesByClientsToResources = new ConcurrentDictionary<string, DateTime>();
        }

        public bool IsRequestAllowed(Client client, string resource)
        {
            string clientIdResourceCompositeKey = $"{client.Id}:{resource}";
            var callTime = DateTime.UtcNow;

            var lastCallTimeByClientToResource = _lastCallTimesByClientsToResources.GetOrAdd(clientIdResourceCompositeKey, DateTime.MinValue);

            bool isRateLimitExceeded = callTime - lastCallTimeByClientToResource < _requiredIntervalBetweenRequestsByClientToResource;
            if (isRateLimitExceeded)
            {
                return false;
            }

            _lastCallTimesByClientsToResources[clientIdResourceCompositeKey] = callTime;
            return true;
        }
    }
}