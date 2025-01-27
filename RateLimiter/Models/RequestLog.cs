
using System;

namespace RateLimiter.Models
{
    public class RequestLog
    {
        public string ClientId { get; set; }
        public string Resource { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
