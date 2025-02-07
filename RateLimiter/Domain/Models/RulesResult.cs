namespace RateLimiter.Domain.Models
{
    public class RulesResult
    {
        public string Message { get; set; }
        public bool Status { get; set; }
        public int RetryAfter { get; set; }
        public RateLimiterStats updatedRateLimiterStats { get; set; }
    }
}
