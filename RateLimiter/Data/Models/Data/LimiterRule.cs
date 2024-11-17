using RateLimiter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Data.Models.Data
{
    public class LimiterRule : BaseModel
    {
        public string Name { get; set; }

        // conditions to look for
        public string? TokenSource { get; set; }
        public int? ResourceStatusId { get; set; }

        // limiter
        public int NumPerTimespan { get; set; }
        public int NumSeconds { get; set; }

    }
}
