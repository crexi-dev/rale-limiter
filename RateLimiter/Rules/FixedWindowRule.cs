using System;
using System.Threading;
using System.Threading.Tasks;
using RateLimiter.Models;

namespace RateLimiter.Rules;

public class FixedWindowRule : IRateLimitRule
{
    private const string AccessForbiddenMessage = "Access forbidden for {0} seconds";

    private readonly int _limit;
    private readonly TimeSpan _window;
    private DateTime _startTime;
    private int _count;
    private readonly object _lock = new();

    public FixedWindowRule(int limit, TimeSpan window)
    {
        _limit = limit;
        _window = window;
        _startTime = DateTime.UtcNow;
        _count = 0;
    }

    public ValueTask<RuleResult> ApplyAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        lock (_lock)
        {
            if (now - _startTime >= _window)
            {
                _startTime = now;
                _count = 1;
                return ValueTask.FromResult(new RuleResult { IsAllowed = true });
            }

            if (_count >= _limit)
            {
                var windowExpirationTime = _startTime + _window;
                var remainingTime = (windowExpirationTime - now).TotalSeconds;

                return ValueTask.FromResult(new RuleResult
                {
                    IsAllowed = false,
                    RuleMessage = string.Format(AccessForbiddenMessage, $"{remainingTime:F2} ")
                });
            }

            _count++;
        }

        return ValueTask.FromResult(new RuleResult { IsAllowed = true });
    }
}