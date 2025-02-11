using RateLimiter.Abstractions;
using RateLimiter.Enums;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RateLimiter.Rules.Algorithms;

public class SlidingWindow : IAmARateLimitAlgorithm
{
    private readonly int _maxRequests;
    private readonly TimeSpan _windowDuration;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ConcurrentDictionary<string, List<DateTime>> _clientTimestamps = new();

    public SlidingWindow(IDateTimeProvider dateTimeProvider, SlidingWindowConfiguration configuration)
    {
        _dateTimeProvider = dateTimeProvider;
        _maxRequests = configuration.MaxRequests;
        _windowDuration = configuration.WindowDuration;
    }

    public string Name { get; init; } = nameof(SlidingWindow);

    public bool IsAllowed(string discriminator)
    {
        var now = _dateTimeProvider.UtcNow();
        var timestamps = _clientTimestamps.GetOrAdd(discriminator, _ => new List<DateTime>());

        lock (timestamps)
        {
            // Remove timestamps older than the current window
            var cutoff = now - _windowDuration;
            timestamps.RemoveAll(t => t < cutoff);

            if (timestamps.Count >= _maxRequests)
                return false;

            timestamps.Add(now); // Add current request timestamp
            return true;
        }
    }

    public RateLimitingAlgorithm Algorithm { get; init; } = RateLimitingAlgorithm.SlidingWindow;
}