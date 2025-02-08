using RateLimiter.Abstractions;

using System;
using System.Collections.Concurrent;

namespace RateLimiter.Rules;

public class FixedWindowRule : IRateLimitRule
{
    private readonly int _maxRequests;
    private readonly TimeSpan _windowDuration;
    private readonly ConcurrentDictionary<string, (int Count, DateTime WindowStart)> _clientWindows;

    public FixedWindowRule(int maxRequests, TimeSpan windowDuration)
    {
        _maxRequests = maxRequests;
        _windowDuration = windowDuration;
        _clientWindows = new ConcurrentDictionary<string, (int, DateTime)>();
    }

    public bool IsAllowed(string discriminator)
    {
        var now = DateTime.UtcNow;

        // Atomically update or create a window for the client
        var window = _clientWindows.AddOrUpdate(
            discriminator,
            (1, now), // New client: start window with 1 request
            (_, existing) => now - existing.WindowStart >= _windowDuration ?
                // Window expired: reset count and start new window
                (1, now) :
                // Still in window: increment
                (existing.Count + 1, existing.WindowStart));

        return window.Count <= _maxRequests;
    }
}