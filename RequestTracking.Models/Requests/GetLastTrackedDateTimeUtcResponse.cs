using RequestTracking.Models.Enums;

namespace RequestTracking.Models.Requests;

public class GetLastTrackedDateTimeUtcResponse
{
    public ResponseCodeEnum ResponseCode { get; set; }
    public string? ResponseMessage { get; set; }
    public DateTime LastTrackedDateTimeUtc { get; set; }
}
