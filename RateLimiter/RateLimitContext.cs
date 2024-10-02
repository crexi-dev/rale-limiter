using System;

namespace RateLimiter
{
    public class RateLimitContext
    {
        public string ClientToken { get; set; }
        public string Resource { get; set; }
        public DateTime RequestTime { get; set; }
        public string Region { get; set; } // US or EU
    }

}
