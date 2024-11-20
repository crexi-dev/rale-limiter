using Cache.Models;
using Cache.Providers;
using RequestTracking.Interfaces;
using RequestTracking.Models;

namespace RequestTracking.Services;

public class CacheTrackingStorageProvider : ITrackingStorageProvider
{
    private readonly ICacheProvider _cacheProvider;
    public CacheTrackingStorageProvider(ICacheProvider cacheProvider)
    {
        _cacheProvider = cacheProvider;
    }

    public async Task AddTrackingItem(string key, object item, double expireAfterSec)
    {
        CacheOptions cacheOptions = new CacheOptions() { CacheExpiryOption = CacheExpiryOptionEnum.Absolute, ExpiryTTLSeconds = expireAfterSec };
        TrackingItem trItem = new TrackingItem() { Item = item };

        await _cacheProvider.Set($"{key}_{Guid.NewGuid()}", trItem, cacheOptions);
    }
    public async Task<List<TrackingItem>> GetByPattern(string pattern)
    {
        List<TrackingItem> items = await _cacheProvider.GetValues<TrackingItem>(pattern);
        return items;
    }
}
