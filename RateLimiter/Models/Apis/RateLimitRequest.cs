using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Models.Apis
{
    /// <summary>
    /// Class for rate checking request.
    /// </summary>
    public class RateLimitRequest
    {
        public string UserId { get; set; }
        public string ResourceId { get; set; }
    }
}
