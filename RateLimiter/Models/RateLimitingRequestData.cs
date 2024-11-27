using System.Collections.Generic;
using System.Net;

namespace RateLimiter.Models
{
    public class RateLimitingRequestData
    {
        public IPAddress? Ip { get; set; }
        public string Action { get; set; } = default!;
        public string Controller { get; set; } = default!;
        public List<string> Parameters { get; set; }
    }
}
