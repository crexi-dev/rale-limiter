using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Models
{
    public class ClientRateLimitConfig
    {
        public string ClientId { get; set; }
        public List<ResourceRateLimitConfig> ResourceLimits { get; set; } = new List<ResourceRateLimitConfig> { };
    }
}
