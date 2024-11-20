using RequestTracking.Models.Enums;

namespace RequestTracking.Models.Requests;

public class GetByPatternResponse
{
    public ResponseCodeEnum ResponseCode { get; set; }
    public string? ResponseMessage { get; set; }
    public List<TrackingItem>? TrackingItems { get; set; }
}
