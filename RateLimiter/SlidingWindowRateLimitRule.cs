using System;
using System.Collections.Concurrent;

public class SlidingWindowRateLimitRule : IRateLimitRule
{
  private readonly int _limit;
  private readonly TimeSpan _windowSize;
  private readonly ConcurrentDictionary<string, ConcurrentQueue<DateTime>> _requestCounts;

  public SlidingWindowRateLimitRule(int limit, TimeSpan windowSize)
  {
    _limit = limit;
    _windowSize = windowSize;
    _requestCounts = new ConcurrentDictionary<string, ConcurrentQueue<DateTime>>();
  }

  public bool IsRequestAllowed(string clientId, string resource, string region, string token)
  {
    var key = $"{clientId}:{resource}:{region}:{token}";
    var now = DateTime.UtcNow;

    var requests = _requestCounts.GetOrAdd(key, new ConcurrentQueue<DateTime>());

    // Remove outdated requests from the queue
    while (requests.TryPeek(out var timestamp) && now - timestamp >= _windowSize)
    {
      requests.TryDequeue(out _);
    }

    if (requests.Count >= _limit)
    {
      // Deny the request if the limit is reached
      return false;
    }

    // Add the new request timestamp
    requests.Enqueue(now);

    return true;
  }
}
