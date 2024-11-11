using System.Collections.Concurrent;

namespace RateLimiter.Repository.TrafficRepository;

public class InMemoryTrafficRepository : ITrafficRepository
{
    private readonly ConcurrentDictionary<TrafficKey, ConcurrentQueue<Traffic>> _traffic = new();
    private readonly object _lock = new();
    #region ITrafficRepository

    public Task RecordTraffic(Traffic traffic)
    {
        lock (_lock)
        {
            TrafficKey key = new(traffic.Resource, traffic.Token);
            if (!_traffic.TryGetValue(key, out ConcurrentQueue<Traffic>? queue))
            {
                queue = new ConcurrentQueue<Traffic>();
                _traffic[key] = queue;
            }

            queue.Enqueue(traffic);
        }

        return Task.CompletedTask;
    }

    public Task<Traffic?> GetLatestTraffic(string token, string resource)
    {
        if (_traffic.TryGetValue(new(resource, token), out ConcurrentQueue<Traffic>? queue) && queue.TryPeek(out Traffic? latestTraffic))
        {
            return Task.FromResult<Traffic?>(latestTraffic);
        }
        return Task.FromResult<Traffic?>(null);
    }

    public Task<IEnumerable<Traffic>> GetTraffic(string token, string resource, TimeSpan? within_span = null)
    {
        if (!_traffic.TryGetValue(new(resource, token), out ConcurrentQueue<Traffic>? queue))
        {
            return Task.FromResult(Enumerable.Empty<Traffic>());
        }

        IEnumerable<Traffic> result = queue
            .Where(t => TrafficIsWithinTimeSpan(t, within_span))
            .AsEnumerable();

        return Task.FromResult(result);
    }

    public Task<int> CountTraffic(string token, string resource, TimeSpan? within_span = null)
    {
        if (!_traffic.TryGetValue(new(resource, token), out ConcurrentQueue<Traffic>? queue))
        {
            return Task.FromResult(0);
        }

        int count = queue
                    .Where(t => TrafficIsWithinTimeSpan(t, within_span))
                    .Sum(t => t.Requests);

        return Task.FromResult(count);
    }

    public Task<int> ExpireTraffic(string token, string resource, TimeSpan? within_span = null)
    {
        if (!_traffic.TryGetValue(new(resource, token), out ConcurrentQueue<Traffic>? queue))
        {
            return Task.FromResult(0);
        }

        int affected = 0;

        while (queue.TryPeek(out Traffic? t) && TrafficIsWithinTimeSpan(t, within_span))
        {
            if (queue.TryDequeue(out _))
            {
                affected++;
            }
        }

        return Task.FromResult(affected);
    }
    #endregion

    #region Utility
    private static bool TrafficIsWithinTimeSpan(Traffic traffic, TimeSpan? span)
    {
        return !span.HasValue || (DateTime.UtcNow - traffic.Time) <= span.Value;
    }
    #endregion Utility
}
