using RequestTracking.Models;

namespace RequestTracking.Interfaces
{
    public interface ITrackingStorageProvider
    {
        void AddTrackedItem(string key, object item, double expireAfterSec);
        (int Count, DateTime LastDateTimeUtc) GetTrackedItemsInfo(string key, DateTime start, DateTime end);
        DateTime GetLastTrackedDateTime(string key);
    }
}