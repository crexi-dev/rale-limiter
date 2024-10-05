using RateLimiter.Data;
using System;

namespace RateLimiter.Rules
{
    /// <summary>
    /// Ensures a minimum time has passed between consecutive requests (e.g., 1 request every 5 seconds).
    /// </summary>
    public class TimeLimitBetweenRequests : IRateLimiterRule
    {
        private readonly TimeSpan _timeBetweenRequests;
        private readonly IRateLimiterDataStore _store;

        public TimeLimitBetweenRequests(TimeSpan timeBetweenRequests, IRateLimiterDataStore store)
        {
            _timeBetweenRequests = timeBetweenRequests;
            _store = store;
        }

        public RateLimiterResult CheckLimit(string clientId, string resource)
        {
            var clientData = _store.GetClientData(clientId, resource);

            if(clientData.RequestCount == 0)
            {
                return RateLimiterResult.Allowed();// Allow if it is a first time request
            }

            if ((DateTime.UtcNow - clientData.LastRequestTime) < _timeBetweenRequests)
            {
                return RateLimiterResult.Denied($"Requests must be at least {_timeBetweenRequests.TotalSeconds} seconds apart.");
            }
            
            return RateLimiterResult.Allowed();
        }
    }
}
