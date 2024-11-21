namespace RequestTracking.Models.Requests;

public class GetTrackedItemsRequest
{
    public string Key { get; set; } = null!;
    public DateTime Start { get; set; }
    public DateTime End { get; set; }

}
