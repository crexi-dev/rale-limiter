using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Models
{
    public class Rule
    {
        [Key]
        public int Id { get; set; }
        public int NumberOfRequestsAllowedPerInterval { get; set; }
        public int Interval { get; set; } // number of seconds
        public int TimeSinceLastRequest { get; set; }

        public bool? NumberOfRequestAllowedPerIntervalActive { get; set; }

        public bool? TimeSinceLastRequestActive { get; set; }
    }
}
