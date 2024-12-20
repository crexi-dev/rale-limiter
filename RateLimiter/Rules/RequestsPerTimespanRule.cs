using RateLimiter.Interfaces;
using System;
using System.Collections.Concurrent;
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
        private readonly ConcurrentDictionary<string, ConcurrentBag<DateTime>> _requestsStorage = new();

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
            var nowDateTime = DateTime.Now;
            var requests = _requestsStorage.GetOrAdd(clientId, new ConcurrentBag<DateTime>());
            requests.Add(nowDateTime);

            var elapsedTimespan = nowDateTime - _timespan;
            var requestsWithinTimespan = requests.Where(request => request > elapsedTimespan).ToList();
            _requestsStorage[clientId] = new ConcurrentBag<DateTime>(requestsWithinTimespan);

            // Allow the request if the number of requests within the timespan is less than or equal to the maximum allowed requests
            return requestsWithinTimespan.Count <= _maxRequests;
        }
    }
}
