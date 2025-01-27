using RateLimiter.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Rules
{
    public class ResourceRateLimitRule : IRateLimitRule
    {
        private readonly Dictionary<string, int> _resourceLimits;
        private readonly Dictionary<string, Queue<DateTime>> _resourceRequests = new();

        public ResourceRateLimitRule(Dictionary<string, int> resourceLimits)
        {
            _resourceLimits = resourceLimits;
        }

        public  bool IsRequestAllowed(string clientId, string resource, string ip)
        {
            if (!_resourceLimits.ContainsKey(resource))
                return true;

            if (!_resourceRequests.ContainsKey(resource))
                _resourceRequests[resource] = new Queue<DateTime>();

            var requests = _resourceRequests[resource];
            var now = DateTime.UtcNow;

            while (requests.Count > 0 && requests.Peek() <= now - TimeSpan.FromMinutes(1))
                requests.Dequeue();

            if (requests.Count < _resourceLimits[resource])
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
