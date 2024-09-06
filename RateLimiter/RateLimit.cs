using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter
{
    public class RateLimit
    {   public DateTime FirstHit { get; set; }
        public int HitCount { get; set; }
    }
}
