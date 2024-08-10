using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Model
{
    public class RequestDTO
    {
        public string CallId { get; set; }
        public string Region { get; set; }
        public DateTime CurrentTime { get; set; }   
    }
}
