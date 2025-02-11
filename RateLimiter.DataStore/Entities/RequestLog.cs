namespace RateLimiter.DataStore.Entities
{
    public class RequestLog
    {
        public int RequestLogId { get; set; }

        public string ClientToken { get; set; }

        public string ResourceName { get; set; }

        public string TimeStampString { get; set; } = string.Empty;

        public int AccessCounts { get; set; } = 1;

        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedTime { get; set; } = null;
    }
}