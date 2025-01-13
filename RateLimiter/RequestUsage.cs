using System;

namespace RateLimiter
{
    public class RequestUsage
    {
        public int RequestCount { get; set; }
        public DateTime WindowStart { get; set; }
        public DateTime LastRequestTime { get; set; }
    }
}
