using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RateLimiter.Models;

namespace RateLimiter.Interfaces
{
    public interface IRateLimitingRule
    {
        RuleResult Check(List<DateTime> LoggedTimes);
        int ReturnMaxRequests();
    }
}
