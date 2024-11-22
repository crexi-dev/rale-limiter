
using RequestTracking.Interfaces;
using RequestTracking.Models.Requests;

namespace RequestTracking.Services;

public class RequestTrackingService : IRequestTrackingService
{
    private readonly ITrackingStorageProvider _storageProvider;
    public RequestTrackingService(ITrackingStorageProvider storageProvider)
    {
        _storageProvider = storageProvider;
    }

    public AddTrackedItemResponse AddTrackedItem(AddTrackedItemRequest request)
    {
        AddTrackedItemResponse response = new AddTrackedItemResponse() { ResponseCode = Models.Enums.ResponseCodeEnum.Success, ResponseMessage = "Success" };
        try
        {
            _storageProvider.AddTrackedItem(request.TrackingId, request.Request, request.ExpireAfterSeconds);
        }
        catch (Exception ex)
        {
            response.ResponseCode = Models.Enums.ResponseCodeEnum.SystemError;
            response.ResponseMessage = ex.Message;
        }
        return response;
    }

    public GetTrackedItemsResponse GetTrackedItemsInfo(GetTrackedItemsRequest request)
    {
        GetTrackedItemsResponse response = new GetTrackedItemsResponse() { ResponseCode = Models.Enums.ResponseCodeEnum.Success, ResponseMessage = "Success" };
        try
        {
            response.Count = _storageProvider.GetTrackedItemsCount(request.Key, request.Start, request.End);
           
        }
        catch (Exception ex)
        {
            response.ResponseCode = Models.Enums.ResponseCodeEnum.SystemError;
            response.ResponseMessage = ex.Message;
        }
        return response;
    }

    public GetLastTrackedDateTimeUtcResponse GetLastTrackedDateTimeUtc(GetLastTrackedDateTimeUtcRequest request)
    {
        GetLastTrackedDateTimeUtcResponse response = new GetLastTrackedDateTimeUtcResponse() { ResponseCode = Models.Enums.ResponseCodeEnum.Success, ResponseMessage = "Success" };
        try
        {
            response.LastTrackedDateTimeUtc = _storageProvider.GetLastTrackedDateTime(request.Key);
        }
        catch (Exception ex)
        {
            response.ResponseCode = Models.Enums.ResponseCodeEnum.SystemError;
            response.ResponseMessage = ex.Message;
        }
        return response;
    }
}
