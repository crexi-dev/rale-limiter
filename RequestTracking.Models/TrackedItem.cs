namespace RequestTracking.Models
{
    public class TrackedItem : IComparable<TrackedItem>
    {
        public string Id { get; set; } = null!;
        public object Item { get; set; } = null!;
        public DateTime CreatedDateTimeUtc { get; set; }
        public DateTime ExpirationDateTimeUtc { get; set; }
        public TrackedItem() {
            CreatedDateTimeUtc = DateTime.UtcNow;
        }
        public bool IsExpired
        {
            get
            {
                return DateTime.UtcNow > ExpirationDateTimeUtc;
            }
        }

        public int CompareTo(TrackedItem? otherItem)
        {
            if (otherItem == null)
            {
                return 1; 
            }

            return CreatedDateTimeUtc.CompareTo(otherItem.CreatedDateTimeUtc);
        }
    }
}
