using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Models.RatePolicies
{
    /// <summary>
    /// Enum for all policy types.
    /// </summary>
    public enum PolicyType
    {
        NoLimit,
        TimeSpan,
        LastCall,
    }
}
