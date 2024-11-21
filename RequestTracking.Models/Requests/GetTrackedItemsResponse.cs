using RequestTracking.Models.Enums;

namespace RequestTracking.Models.Requests;

public class GetTrackedItemsResponse
{
    public ResponseCodeEnum ResponseCode { get; set; }
    public string? ResponseMessage { get; set; }
    public int Count { get; set; }
    public DateTime LastTrackedDateTimeUtc { get; set; }
}
