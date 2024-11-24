using Crexi.API.Common.RateLimiter.Interfaces;
using Crexi.API.Common.RateLimiter.Models;
using System;
using System.Collections.Concurrent;

namespace Crexi.API.Common.RateLimiter.Rules
{
    public class FixedWindowRateLimitRule : IRateLimitRule
    {
        private readonly int _maxRequestsPerClientToResource;
        private readonly TimeSpan _timeWindow;
        private readonly ConcurrentDictionary<string, (int RequestCountMadeByClientToResource, DateTime RateLimitWindowStart)> _clientResourseRequestCounters;

        public FixedWindowRateLimitRule(int maxRequestsPerClient, TimeSpan timeWindow)
        {
            _maxRequestsPerClientToResource = maxRequestsPerClient;
            _timeWindow = timeWindow;
            _clientResourseRequestCounters = new ConcurrentDictionary<string, (int, DateTime)>();
        }

        public bool IsRequestAllowed(Client client, string resource)
        {
            string clientIdResourceCompositeKey = $"{client.Id}:{resource}";
            var currentClientRequestTime = DateTime.UtcNow;

            var reqCounterForClientToResource = _clientResourseRequestCounters.GetOrAdd(clientIdResourceCompositeKey, (0, currentClientRequestTime));

            //  Determine if current time has moved past the current rate limit window. If it has, reqCounter should be reset to start a new window.
            var clientRequestInNewWindow = currentClientRequestTime - reqCounterForClientToResource.RateLimitWindowStart >= _timeWindow;
            if (clientRequestInNewWindow)
            {
                reqCounterForClientToResource = (0, currentClientRequestTime);
            }

            var clientToResourceLimitReached = reqCounterForClientToResource.RequestCountMadeByClientToResource >= _maxRequestsPerClientToResource;
            if (clientToResourceLimitReached)
            {
                _clientResourseRequestCounters[clientIdResourceCompositeKey] = reqCounterForClientToResource;
                return false;
            }

            // Reset the counter if the current time has moved past the current rate limit window.
            reqCounterForClientToResource = (reqCounterForClientToResource.RequestCountMadeByClientToResource + 1, reqCounterForClientToResource.RateLimitWindowStart);
            _clientResourseRequestCounters[clientIdResourceCompositeKey] = reqCounterForClientToResource;

            return true;
        }
    }
}