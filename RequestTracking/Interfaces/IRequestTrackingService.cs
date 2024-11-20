using RequestTracking.Models.Requests;

namespace RequestTracking.Interfaces
{
    public interface IRequestTrackingService
    {
        Task<AddTrackingResponse> AddTrackingAsync(AddTrackingRequest request);
        Task<GetByPatternResponse> GetTrackingResponseAsync(GetByPatternRequest request);
    }
}