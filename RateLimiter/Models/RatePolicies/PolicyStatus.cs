using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Models.RatePolicies
{
    /// <summary>
    /// Class for policy checking status.
    /// </summary>
    public class PolicyStatus
    {
        public bool IsConforming { get; set; }
        public string? NotConformingReason { get; set; }
    }
}
