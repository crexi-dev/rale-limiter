using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Models
{
    public class Request : BaseModel
    {
        public int UserId { get; set; }
        public int ResourceId { get; set; }
        public DateTime RequestDate { get; set; }
        public bool WasHandled { get; set; }

        public User User { get; set; }
        public Resource Resource { get; set; }
    }
}
