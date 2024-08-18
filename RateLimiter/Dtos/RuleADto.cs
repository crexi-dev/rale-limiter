namespace RateLimiter.Dtos
{
    public class RuleADto
    {
        public DateTime LastCallDateTime { get; set; }
        public int RequestCount { get; set; }
    }
}
