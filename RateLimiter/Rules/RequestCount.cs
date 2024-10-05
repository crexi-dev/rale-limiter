using RateLimiter.Data;
using System;

namespace RateLimiter.Rules
{
    /// <summary>
    /// Limits the number of requests within a given timespan (e.g., 100 requests per hour).
    /// </summary>
    public class RequestCount : IRateLimiterRule
    {
        private readonly int _maxRequests;
        private readonly TimeSpan _timeSpan;
        private readonly IRateLimiterDataStore _store;

        public RequestCount(int maxRequests, TimeSpan timeSpan, IRateLimiterDataStore store)
        {
            _maxRequests = maxRequests;
            _timeSpan = timeSpan;
            _store = store;
        }

        public RateLimiterResult CheckLimit(string clientId, string resource)
        {
            var clientData = _store.GetClientData(clientId, resource);

            if (clientData == null || (DateTime.UtcNow - clientData.StartTime) > _timeSpan)
            {
                _store.ResetClientData(clientId, resource);
                clientData = _store.GetClientData(clientId, resource);
            }

            if (clientData.RequestCount > _maxRequests)
            {
                return RateLimiterResult.Denied($"Request limit of {_maxRequests} per {_timeSpan.TotalMinutes} minutes exceeded.");
            }

            return RateLimiterResult.Allowed();
        }
    }
}
