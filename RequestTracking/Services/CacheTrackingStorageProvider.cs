using RequestTracking.Interfaces;
using RequestTracking.Models;
using System.Collections.Concurrent;

namespace RequestTracking.Services;

public class CacheTrackingStorageProvider : ITrackingStorageProvider
{
    // probably shoud be intialized from configuration 
    private readonly TimeSpan constDefaultCleanupTimeSpan = TimeSpan.FromSeconds(60);
    private ConcurrentDictionary<string, List<TrackedItem>> _cache;
    private readonly Timer _cleanupTimer;

    public CacheTrackingStorageProvider()
    {
        _cleanupTimer = new Timer(CleanupExpiredItems, null, TimeSpan.Zero, constDefaultCleanupTimeSpan);
        _cache = new ConcurrentDictionary<string, List<TrackedItem>>();
    }
   
    public void AddTrackedItem(string key, object item, double expireAfterSec)
    {
        var items = _cache.GetOrAdd(key, _ => new List<TrackedItem>());

        lock (items) 
        {
            TrackedItem trItem = new TrackedItem() { Item = item, ExpirationDateTimeUtc = DateTime.Now.AddSeconds(expireAfterSec) };
            items.Add(trItem);
        }
    }

    public (int Count, DateTime LastDateTimeUtc) GetTrackedItemsInfo(string key, DateTime start, DateTime end)
    {
        if (_cache.TryGetValue(key, out var items))
        {
            lock (items) 
            {
                int startIndex = items.BinarySearch(new TrackedItem { CreatedDateTimeUtc = start },   Comparer<TrackedItem>.Default);
                int endIndex = items.BinarySearch(new TrackedItem { CreatedDateTimeUtc = end }, Comparer<TrackedItem>.Default);

                startIndex = startIndex < 0 ? ~startIndex : startIndex;
                endIndex = endIndex < 0 ? ~endIndex : endIndex;

                return (endIndex - startIndex, items[^1].CreatedDateTimeUtc);
            }
        }
        return (0, DateTime.MinValue);

    }

    public DateTime GetLastTrackedDateTime(string key)
    {
        if (_cache.TryGetValue(key, out var items))
        {
            lock (items)
            {
                return items[^1].CreatedDateTimeUtc;
            }
        }
        return DateTime.MinValue;
    }

    private void CleanupExpiredItems(object? state)
    {
        foreach (var kvp in _cache)
        {
            if (_cache.TryGetValue(kvp.Key, out var list))
            {
                lock (list)
                {
                    list.RemoveAll(obj => obj.IsExpired);
                }
            }
        }
    }

}
