using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Models.Entities
{
    /// <summary>
    /// Class for policy entity.
    /// </summary>
    public class Policy
    {
        public string PolicyId { get; set; }
        public string PolicyName { get; set; }
        public string PolicyJson { get; set; }
    }
}
