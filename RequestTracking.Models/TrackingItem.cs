namespace RequestTracking.Models
{
    public class TrackingItem
    {
        public string Id { get; set; } = null!;
        public object Item { get; set; } = null!;
        public DateTime UtcDateTime { get; set; }
        public TrackingItem() {
            UtcDateTime = DateTime.UtcNow;
        }

    }
}
