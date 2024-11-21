using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Data.Filters
{
    public class RequestsFilter
    {
        public int? RequestId { get; set; }
        public int? UserId { get; set; }
    }
}
