using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Models.Apis
{
    /// <summary>
    /// Response class for rate limit checking request.
    /// </summary>
    public class RateLimiteResponse
    {
        public bool Success { get; set; }
        public List<string>? Errors { get; set; }
    }
}
