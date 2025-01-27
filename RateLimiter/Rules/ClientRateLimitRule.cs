using RateLimiter.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Rules
{
    public class ClientRateLimitRule : IRateLimitRule
    {
        private readonly int _requestLimit;
        private readonly TimeSpan _timeWindow;
        private readonly Dictionary<string, Queue<DateTime>> _clientRequests = new();

        public ClientRateLimitRule(int requestLimit, TimeSpan timeWindow)
        {
            _requestLimit = requestLimit;
            _timeWindow = timeWindow;
        }

        public  bool IsRequestAllowed(string clientId, string resource, string ip)
        {
            if (!_clientRequests.ContainsKey(clientId))
                _clientRequests[clientId] = new Queue<DateTime>();

            var requests = _clientRequests[clientId];
            var now = DateTime.UtcNow;

            while (requests.Count > 0 && requests.Peek() <= now - _timeWindow)
                requests.Dequeue();

            if (requests.Count < _requestLimit)
            {
                requests.Enqueue(now);
                return true;
            }

            return false;
        }

        public bool IsRequestAllowed(string clientId, string resource)
        {
           return this.IsRequestAllowed(clientId, resource, string.Empty);
        }
    }
}
