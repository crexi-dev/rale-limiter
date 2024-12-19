using RateLimiter.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RateLimiter.Rules
{
    /// <summary>
    /// A rate limiting rule that allows a maximum number of requests within a specified timespan.
    /// </summary>
    public class RequestsPerTimespanRule : IRateLimitingRule
    {
        private readonly int _maxRequests;
        private readonly TimeSpan _timespan;
        private readonly Dictionary<string, List<DateTime>> _requestsStorage = new Dictionary<string, List<DateTime>>(); // TODO: Implement a proper storage mechanism

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestsPerTimespanRule"/> class.
        /// </summary>
        /// <param name="maxRequests">Maximum number of requests allowed</param>
        /// <param name="timespan">The timespan of which we limit the maximum number of requests</param>
        public RequestsPerTimespanRule(int maxRequests, TimeSpan timespan)
        {
            _maxRequests = maxRequests;
            _timespan = timespan;
        }

        /// <summary>
        /// Determines whether a request is allowed for the given client, based on the maximum number of requests within the specified timespan.
        /// </summary>
        /// <param name="clientId">Requesting client id</param>
        /// <returns>Result if request is allowed</returns>
        public bool IsRequestAllowed(string clientId)
        {
            if (!_requestsStorage.ContainsKey(clientId))
            {
                _requestsStorage[clientId] = new List<DateTime>();
            }

            var nowDateTime = DateTime.Now;
            _requestsStorage[clientId].Add(nowDateTime);

            var elapsedTimespan = nowDateTime - _timespan;
            var requestsWithinTimespan = _requestsStorage[clientId].Where(request => request > nowDateTime - _timespan).ToList();

            // Allow the request if the number of requests within the timespan is less than or equal to the maximum allowed requests
            return requestsWithinTimespan.Count <= _maxRequests;
        }
    }
}
