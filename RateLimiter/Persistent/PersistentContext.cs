using RateLimiter.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Persistent
{
    /// <summary>
    /// Class for a memory based persistent context.
    /// </summary>
    public class PersistentContext
    {
        public List<User> Users { get; set; }
        public List<Resource> Resources { get; set; }
        public List<Policy> Policies { get; set; }
        public List<UserResourcePolicy> UserResourcePolicies { get; set; }
        public List<UserActivity> UserActivities { get; set; }
    }
}
