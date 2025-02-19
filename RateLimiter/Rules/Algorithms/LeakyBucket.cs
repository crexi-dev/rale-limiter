using RateLimiter.Abstractions;
using RateLimiter.Enums;

using System;
using System.Collections.Concurrent;

namespace RateLimiter.Rules.Algorithms;

public class LeakyBucket : IRateLimitAlgorithm
{
    private readonly int _capacity;
    private readonly TimeSpan _leakInterval;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ConcurrentDictionary<string, BucketState> _buckets;

    public string Name { get; init; } = nameof(LeakyBucket);

    public LeakyBucket(
        IDateTimeProvider dateTimeProvider,
        LeakyBucketConfiguration configuration)
    {
        _dateTimeProvider = dateTimeProvider;
        _capacity = configuration.Capacity;
        _leakInterval = configuration.Interval;
    }

    public bool IsAllowed(string discriminator)
    {
        var bucket = _buckets.GetOrAdd(discriminator, _ => new BucketState());

        lock (bucket.Lock)
        {
            var now = _dateTimeProvider.UtcNow();
            var timeElapsed = now - bucket.LastLeakTime;

            // Calculate how many requests have "leaked out" since the last check
            var leakedRequests = (int)(timeElapsed.Ticks / _leakInterval.Ticks);

            if (leakedRequests > 0)
            {
                bucket.CurrentCount = Math.Max(0, bucket.CurrentCount - leakedRequests);
                // Adjust last leak time to account for partial intervals
                bucket.LastLeakTime = now.AddTicks(-(timeElapsed.Ticks % _leakInterval.Ticks));
            }

            // Allow request if bucket isn't full
            if (bucket.CurrentCount >= _capacity)
                return false;

            bucket.CurrentCount++;

            return true;
        }
    }

    public AlgorithmType AlgorithmType { get; init; } = AlgorithmType.LeakyBucket;

    private class BucketState
    {
        public int CurrentCount { get; set; }
        public DateTime LastLeakTime { get; set; } = DateTime.MinValue;
        public object Lock { get; } = new object();
    }
}