using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Models.Entities
{
    /// <summary>
    /// Class for policies for particular user and response pair.
    /// </summary>
    public class UserResourcePolicy
    {
        public string UserId { get; set; }
        public string ResourceId { get; set; }
        public List<string> Policies { get; set; }
    }
}
