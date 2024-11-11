using System.Collections.Concurrent;

namespace Services.Common.RateLimitRules;

public class RequestPerTimespanRule : IRateLimitRule
{
    private readonly int _limit;
    private readonly TimeSpan _timespan;
    private readonly ConcurrentDictionary<Guid, (int count, DateTime start) > _requestCounts;

    public RequestPerTimespanRule(int limit, TimeSpan timespan)
    {
        _limit = limit;
        _timespan = timespan;
        _requestCounts = new ConcurrentDictionary<Guid, (int, DateTime)>();
    }

    public bool IsRequestAllowed(Guid token)
    {
        var now = DateTime.UtcNow;
        var (count, start) = _requestCounts.GetOrAdd(token, (0, now));

        if (now - start > _timespan)
        {
            _requestCounts[token] = (1, now);
            return true;
        }

        if (count >= _limit) return false;
        _requestCounts[token] = (count + 1, start);
        return true;

    }
}