
using RequestTracking.Models.Enums;

namespace RequestTracking.Models.Requests;

public class AddTrackedItemResponse
{
    public ResponseCodeEnum ResponseCode { get; set; }
    public string? ResponseMessage { get; set; }
}
