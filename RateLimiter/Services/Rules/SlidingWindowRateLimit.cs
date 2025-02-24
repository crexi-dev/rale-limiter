using RateLimiter.Interfaces;
using RateLimiter.Models;

namespace RateLimiter.Services.Rules
{

    /// <summary>
    /// Example: 5 requests per 1-minute window
    /// 4:27:10 - Allowed (1/5)  
    /// 4:27:20 - Allowed (2/5)  
    /// 4:27:30 - Allowed (3/5)  
    /// 4:27:45 - Allowed (4/5)  
    /// 4:27:55 - Allowed (5/5)  
    /// 4:27:58 - Blocked! (Limit reached, must wait)  
    ///
    /// **When requests expire:**
    /// - At **4:28:10**, the request from **4:27:10** expires → 4:28:10 now allowed  
    /// </summary>
    public class SlidingWindowRateLimit : IRateLimitRule
    {
        private readonly int _maxRequests;
        private readonly TimeSpan _timeWindow;
        private readonly Dictionary<string, List<DateTime>> _clientRequestHistory = new();

        public SlidingWindowRateLimit(int maxRequests, TimeSpan timeWindow)
        {
            _maxRequests = maxRequests;
            _timeWindow = timeWindow;
        }

        public RateLimitResult IsRequestAllowed(string clientId)
        {
            lock (_clientRequestHistory)
            {
                DateTime now = DateTime.UtcNow;
                DateTime expirationThreshold = now - _timeWindow;

                // Get or create request history for client
                if (!_clientRequestHistory.TryGetValue(clientId, out var requestTimes))
                {
                    requestTimes = new List<DateTime>();
                    _clientRequestHistory[clientId] = requestTimes;
                }

                // Remove expired timestamps
                requestTimes.RemoveAll(time => time < expirationThreshold);

                // Allow request if within limit
                if (requestTimes.Count < _maxRequests)
                {
                    requestTimes.Add(now);
                    return new RateLimitResult { IsAllowed = true, RetryAfter = TimeSpan.Zero };
                }

                // Rate limit exceeded, calculate retry time
                TimeSpan retryAfter = (requestTimes[0] + _timeWindow) - now;
                return new RateLimitResult { IsAllowed = false, RetryAfter = retryAfter };
            }
        }
    }
}
