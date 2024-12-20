using RateLimiter.Interfaces;
using System.Collections.Concurrent;

namespace RateLimiter.Rules
{
    /// <summary>
    /// A rate limiting rule that limits the number of concurrent requests.
    /// </summary>
    public class ConcurrentRequestLimitRule : IRateLimitingRule
    {
        private readonly int _maxConcurrentRequests;
        private readonly ConcurrentDictionary<string, int> _requestsStorage = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentRequestLimitRule"/> class.
        /// </summary>
        /// <param name="maxConcurrentRequests"></param>
        public ConcurrentRequestLimitRule(int maxConcurrentRequests)
        {
            _maxConcurrentRequests = maxConcurrentRequests;
        }

        /// <summary>
        /// Determines whether a request is allowed for the given client, based on the maximum number of concurrent requests.
        /// </summary>
        /// <param name="clientId">Requesting client id</param>
        /// <returns>Result if request is allowed</returns>
        public bool IsRequestAllowed(string clientId)
        {
            var currentRequests = _requestsStorage.GetOrAdd(clientId, 0);

            if (currentRequests < _maxConcurrentRequests)
            {
                _requestsStorage[clientId] = currentRequests + 1;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Notifies the rule that a request has been completed.
        /// </summary>
        /// <param name="clientId"></param>
        public void RequestCompleted(string clientId)
        {
            if (_requestsStorage.ContainsKey(clientId))
            {
                _requestsStorage[clientId]--;
            }
        }
    }
}
