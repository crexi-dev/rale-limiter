using System;

namespace RateLimiter.Models
{
    public class RequestModel
    {
        public string Resource { get; set; } = string.Empty;
        public DateTime RequestTime { get; set; }
    }
}