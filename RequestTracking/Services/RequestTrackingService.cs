
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

    public async Task<AddTrackingResponse> AddTrackingAsync(AddTrackingRequest request)
    {
        AddTrackingResponse response = new AddTrackingResponse() { ResponseCode = Models.Enums.ResponseCodeEnum.Success, ResponseMessage = "Success" };
        try
        {
            await _storageProvider.AddTrackingItem(request.TrackingId, request.Request, request.ExpireAfterSeconds);
        }
        catch (Exception ex)
        {
            response.ResponseCode = Models.Enums.ResponseCodeEnum.SystemError;
            response.ResponseMessage = ex.Message;
        }
        return response;
    }

    public async Task<GetByPatternResponse> GetTrackingResponseAsync(GetByPatternRequest request)
    {
        GetByPatternResponse response = new GetByPatternResponse() { ResponseCode = Models.Enums.ResponseCodeEnum.Success, ResponseMessage = "Success" };
        try
        {
            response.TrackingItems = await _storageProvider.GetByPattern(request.RequestIdPattern);
        }
        catch (Exception ex)
        {
            response.ResponseCode = Models.Enums.ResponseCodeEnum.SystemError;
            response.ResponseMessage = ex.Message;
        }
        return response;
    }
}
