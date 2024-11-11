using System.Collections.Concurrent;

namespace Services.Common.RateLimitRules;

public class TimeSinceLastCallRule : IRateLimitRule
{
    private readonly TimeSpan _minInterval;
    private readonly ConcurrentDictionary<Guid, DateTime> _lastRequestTime;

    public TimeSinceLastCallRule(TimeSpan minInterval)
    {
        _minInterval = minInterval;
        _lastRequestTime = new ConcurrentDictionary<Guid, DateTime>();
    }

    public bool IsRequestAllowed(Guid token)
    {
        var now = DateTime.UtcNow;
        if (_lastRequestTime.TryGetValue(token, out var lastRequest) && (now - lastRequest) < _minInterval)
        {
            return false;
        }
        _lastRequestTime[token] = now;
        return true;
    }
}
