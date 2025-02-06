namespace RateLimiter.Domain.Models
{
    public class Configurations
    {
        public int RequestsPerTimespan { get; set; }
        public int RequestTimespan { get; set; }
        public int TimespanSinceLastCall { get; set; }
    }
}
