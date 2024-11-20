using RequestTracking.Models;

namespace RequestTracking.Interfaces
{
    public interface ITrackingStorageProvider
    {
        Task AddTrackingItem(string key, object item, double expireAfterSec);
        Task<List<TrackingItem>> GetByPattern(string pattern);
    }
}