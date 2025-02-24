namespace RateLimiter.Models
{
    public class RateLimitEntry
    {
        public int Count { get; set; }
        public DateTime ResetTime { get; set; }
    }
}
