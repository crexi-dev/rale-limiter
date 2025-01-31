using RateLimiter.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RateLimiter.Rules;

public class SlidingWindowRule : IRateLimitRule
{
    private const string AccessForbiddenMessage = "Access forbidden for {0} seconds";
    private readonly int _limit;
    private readonly TimeSpan _window;
    private readonly Queue<DateTime> _timestamps = new();
    private readonly object _lock = new();

    public SlidingWindowRule(int limit, TimeSpan window)
    {
        _limit = limit;
        _window = window;
    }

    public ValueTask<RuleResult> ApplyAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        lock (_lock)
        {
            while (_timestamps.Count > 0 && now - _timestamps.Peek() >= _window)
            {
                _timestamps.Dequeue();
            }

            if (_timestamps.Count >= _limit)
            {
                var oldestRequestTime = _timestamps.Peek();
                var remainingTime = (_window - (now - oldestRequestTime)).TotalSeconds;

                return ValueTask.FromResult(new RuleResult
                {
                    IsAllowed = false,
                    RuleMessage = string.Format(AccessForbiddenMessage, $"{remainingTime:F2} ")
                });
            }

            _timestamps.Enqueue(now);
        }

        return ValueTask.FromResult(new RuleResult { IsAllowed = true });
    }
}
