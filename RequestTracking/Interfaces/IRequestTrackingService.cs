using RequestTracking.Models.Requests;

namespace RequestTracking.Interfaces
{
    public interface IRequestTrackingService
    {
        AddTrackedItemResponse AddTrackedItem(AddTrackedItemRequest request);
        GetTrackedItemsResponse GetTrackedItemsInfo(GetTrackedItemsRequest request);
        GetLastTrackedDateTimeUtcResponse GetLastTrackedDateTimeUtc(GetLastTrackedDateTimeUtcRequest request);
    }
}