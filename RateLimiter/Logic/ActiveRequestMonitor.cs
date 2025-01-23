namespace Crexi.RateLimiter.Logic;

/// <summary>
/// Container to track the requestIDs for a single client ID
/// </summary>
public class ActiveRequestMonitor
{
    private readonly ConcurrentDictionary<string, DateTime> _activeRequestIds = new();

    public long ActiveRequestCount => _activeRequestIds.Count;

    public void AddRequest(string requestId, DateTime requestTime)
    {
        _activeRequestIds.TryAdd(requestId, requestTime);
    }

    public void RemoveRequest(string requestId)
    {
        _activeRequestIds.TryRemove(requestId, out _);
    }
}