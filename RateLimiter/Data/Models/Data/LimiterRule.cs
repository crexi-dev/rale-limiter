namespace RateLimiter.Data.Models
{
    public class LimiterRule : BaseModel
    {
        public string Name { get; set; }

        // conditions to look for
        public string? TokenSource { get; set; }
        public int? ResourceStatusId { get; set; }

        // limiter
        public int NumPerTimespan { get; set; }
        public int NumSeconds { get; set; }

    }
}
