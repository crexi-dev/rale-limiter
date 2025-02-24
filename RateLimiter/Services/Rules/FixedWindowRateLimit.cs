using RateLimiter.Interfaces;
using RateLimiter.Models;

/// <summary>
//  Max Requests: 5 , Time Window: 1 minute
/// 4:27:10 - Allowed (1/5)  
/// 4:27:20 - Allowed (2/5)  
/// 4:27:30 - Allowed (3/5)  
/// 4:27:45 - Allowed (4/5)  
/// 4:27:55 - Allowed (5/5)  
/// 4:27:58 - Blocked! (Wait until 4:28:10)  
/// At 4:28:10, requests reset.
/// </summary>
public class FixedWindowRateLimit : IRateLimitRule
{
    private readonly int _maxRequests;
    private readonly TimeSpan _timeWindow;
    private readonly Dictionary<string, RateLimitEntry> _clientRequestCounts = new();

    public FixedWindowRateLimit(int maxRequests, TimeSpan timeWindow)
    {
        _maxRequests = maxRequests;
        _timeWindow = timeWindow;
    }

    public RateLimitResult IsRequestAllowed(string clientId)
    {
        lock (_clientRequestCounts)
        {
            DateTime now = DateTime.UtcNow;

            // Remove expired entries to free up memory
            List<string> expiredClients = new();
            foreach (var kvp in _clientRequestCounts)
            {
                if (now >= kvp.Value.ResetTime)
                    expiredClients.Add(kvp.Key);
            }
            foreach (var client in expiredClients) { 
                _clientRequestCounts.Remove(client);
            }

            if (!_clientRequestCounts.ContainsKey(clientId) || now >= _clientRequestCounts[clientId].ResetTime)
            {
                _clientRequestCounts[clientId] = new RateLimitEntry() { Count = 1, ResetTime = now + _timeWindow };
                return new RateLimitResult { IsAllowed = true, RetryAfter = TimeSpan.Zero };
            }

            var entry = _clientRequestCounts[clientId];

            if (entry.Count < _maxRequests)
            {
                _clientRequestCounts[clientId] = new RateLimitEntry() { Count = entry.Count + 1, ResetTime = entry.ResetTime };
                return new RateLimitResult { IsAllowed = true, RetryAfter = TimeSpan.Zero };
            }

            return new RateLimitResult
            {
                IsAllowed = false,
                RetryAfter = entry.ResetTime - now
            };
        }
    }
}
