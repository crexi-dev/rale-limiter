using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter
{
    public class FixedWindowRule : IRateLimitRule
    {
        private readonly int _maxRequests;
        private readonly TimeSpan _windowSize;
        private readonly Dictionary<string, (int Count, DateTime WindowStart)> _requestCounts = new();

        public FixedWindowRule(int maxRequests, TimeSpan windowSize)
        {
            _maxRequests = maxRequests;
            _windowSize = windowSize;
        }

        public bool IsRequestAllowed(RateLimitContext context)
        {
            if (!_requestCounts.ContainsKey(context.ClientToken))
            {
                _requestCounts[context.ClientToken] = (0, DateTime.UtcNow);
            }

            var (count, windowStart) = _requestCounts[context.ClientToken];
            var now = DateTime.UtcNow;

            if (now - windowStart > _windowSize)
            {
                windowStart = now;
                count = 0;
            }

            if (count < _maxRequests)
            {
                _requestCounts[context.ClientToken] = (count + 1, windowStart);
                return true;
            }

            return false;
        }
    }

}
