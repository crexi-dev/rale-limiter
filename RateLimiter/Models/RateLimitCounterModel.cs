namespace RateLimiter.Models
{
    public class RateLimitCounterModel
    {
        public RateLimitCounterModel(uint count, long requestTime)
        {
            RequestCount = count;
            RequestTime = requestTime;
        }

        public uint RequestCount { get; set; }

        public long RequestTime { get; set; }
    }
}
