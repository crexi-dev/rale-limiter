using System;
using System.Collections.Concurrent;
using System.Threading;
using RateLimiter.Contracts;

namespace RateLimiter.Services;

public class XRequestsPerTimespanRule(int maxRequests, TimeSpan _timeSpan) : IRateLimitRule
{
    private readonly ConcurrentDictionary<string, ConcurrentQueue<DateTime>> requestsCache = new();

    public bool IsRequestAllowed(string clientToken)
    {
        var now = DateTime.UtcNow;
        requestsCache.GetOrAdd(clientToken, new ConcurrentQueue<DateTime>());
        var requestTimeQueue = requestsCache[clientToken];

        while (requestTimeQueue.TryPeek(out DateTime timeSpan) && (now - timeSpan) > _timeSpan)
        {
            requestTimeQueue.TryDequeue(out _);
        }

        if (requestTimeQueue.Count < maxRequests)
        {
            requestTimeQueue.Enqueue(now);
            return true;
        }

        return false;
    }
}