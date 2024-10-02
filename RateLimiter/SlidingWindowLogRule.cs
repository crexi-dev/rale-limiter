using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter
{
    public class SlidingWindowLogRule : IRateLimitRule
    {
        private readonly int _maxRequests;
        private readonly TimeSpan _windowSize;
        private readonly Dictionary<string, List<DateTime>> _requestLogs = new();

        public SlidingWindowLogRule(int maxRequests, TimeSpan windowSize)
        {
            _maxRequests = maxRequests;
            _windowSize = windowSize;
        }

        public bool IsRequestAllowed(RateLimitContext context)
        {
            if (!_requestLogs.ContainsKey(context.ClientToken))
            {
                _requestLogs[context.ClientToken] = new List<DateTime>();
            }

            var now = DateTime.UtcNow;
            var log = _requestLogs[context.ClientToken];
            log.RemoveAll(timestamp => now - timestamp > _windowSize);

            if (log.Count < _maxRequests)
            {
                log.Add(now);
                return true;
            }

            return false;
        }
    }

}
