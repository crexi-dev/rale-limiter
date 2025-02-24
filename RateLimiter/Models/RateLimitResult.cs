namespace RateLimiter.Models
{
    public class RateLimitResult
    {
        public bool IsAllowed { get; set; }
        public TimeSpan RetryAfter { get; set; }
    }
}
