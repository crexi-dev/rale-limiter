using System;
using System.Collections.Concurrent;

public class TokenRateLimitRule : IRateLimitRule
{
  private readonly int _limit;
  private readonly TimeSpan _timeSpan;
  private readonly ConcurrentDictionary<string, (DateTime startTime, int count)> _requestCounts;

  public TokenRateLimitRule(int limit, TimeSpan timeSpan)
  {
    _limit = limit;
    _timeSpan = timeSpan;
    _requestCounts = new ConcurrentDictionary<string, (DateTime, int)>();
  }

  public bool IsRequestAllowed(string clientId, string resource, string region, string token)
  {
    var key = $"{token}:{resource}:{region}";
    var now = DateTime.UtcNow;

    var result = _requestCounts.AddOrUpdate(key,
        (now, 1),  // Start new count if no previous record exists
        (k, existing) =>
        {

          // Reset count if time window has passed
          if (now - existing.startTime >= _timeSpan)
          {
            return (now, 1);
          }

          // Increment count and keep count + 1 even if over the limit
          if (existing.count >= _limit)
          {
            return (existing.startTime, existing.count + 1);  // Increment but deny request
          }

          // Increment within limit
          return (existing.startTime, existing.count + 1);
        });

    // Return true only if the count is within the limit
    bool isAllowed = result.count <= _limit;
    return isAllowed;
  }
}
