using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Rules
{
    public interface IRateLimitRule
    {
        bool IsWithinLimit(string userId);
    }
}
