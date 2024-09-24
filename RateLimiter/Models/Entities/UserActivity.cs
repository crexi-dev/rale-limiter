using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Models.Entities
{
    /// <summary>
    /// Class for storing user activity.
    /// </summary>
    public class UserActivity
    {
        public string UserId { get; set; }
        public string ResourceId { get; set; }
        public DateTime AccessTime { get; set; }
    }
}
