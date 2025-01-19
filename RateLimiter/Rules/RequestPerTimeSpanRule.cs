using RateLimiter.Store;
using System;
using System.Linq;

namespace RateLimiter.Rules
{
    internal class RequestPerTimeSpanRule : IRateLimiterRule
    {
        private readonly IDataStore _dataStore;
        private readonly int _maxRequestsAllowed;
        private readonly int _timeSpanInSeconds;

        public RequestPerTimeSpanRule(
            IDataStore dataStore,
            int maxRequestsAllowed,
            int timeSpanInSeconds)
        {
            _dataStore = dataStore;
            _maxRequestsAllowed = maxRequestsAllowed;
            _timeSpanInSeconds = timeSpanInSeconds;
        }

        public bool IsAllowed(string token, string uri)
        {
            var requstTimestamp = DateTime.UtcNow;
            var requestsAfterTimeStamp = requstTimestamp.AddSeconds(_timeSpanInSeconds * -1);

            var requests = _dataStore.GetClientRequests(token, requestsAfterTimeStamp);

            if (requests?.Count() > _maxRequestsAllowed)
            {
                return false;
            }

            _dataStore.AddClientRequest(token, uri);

            throw new NotImplementedException();
        }
    }
}
