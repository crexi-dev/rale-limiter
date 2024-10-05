using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Model
{
    public class ClientRateLimiterData
    {
        public DateTime StartTime { get; set; }
        public DateTime LastRequestTime { get; set; }
        public int RequestCount { get; set; }
    }
}
