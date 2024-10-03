using System;
using System.Collections.Concurrent;

public class FixedWindowRateLimitRule : IRateLimitRule
{
  private readonly int _limit; // Maximum number of requests allowed
  private readonly TimeSpan _timeSpan; // Time window for the rate limit
  private readonly ConcurrentDictionary<string, (DateTime startTime, int count)> _requestCounts;

  public FixedWindowRateLimitRule(int limit, TimeSpan timeSpan)
  {
    _limit = limit;
    _timeSpan = timeSpan;
    _requestCounts = new ConcurrentDictionary<string, (DateTime, int)>();
  }

  public bool IsRequestAllowed(string clientId, string resource, string region, string token)
  {
    var key = $"{clientId}:{resource}:{region}:{token}";
    var now = DateTime.UtcNow;

    var result = _requestCounts.AddOrUpdate(key,
        (now, 1), // If the key doesn't exist, initialize with count 1
        (k, existing) =>
        {
          if (now - existing.startTime >= _timeSpan)
          {
            return (now, 1); // Reset to 1 if the time window has passed
          }

          if (existing.count >= _limit)
          {
            // Prevent further increments and deny access once the limit is reached
            return (existing.startTime, existing.count + 1); // Increment and return
          }

          // Only increment if below the limit
          return (existing.startTime, existing.count + 1);
        });

    // The count should be allowed up to the limit
    return result.count <= _limit;
  }

}
