
namespace RequestTracking.Models.Requests;

public class AddTrackingRequest
{
    public string TrackingId { get; set; } = null!;
    public object Request { get; set; } = null!;
    public double ExpireAfterSeconds { get;set; }
}
