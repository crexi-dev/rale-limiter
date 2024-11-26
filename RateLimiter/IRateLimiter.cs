using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter
{
    public interface IRateLimiter
    {
        int RequestLimit { get; }
        TimeSpan Interval { get; }
        bool CheckRequest(string limiterToken);
    }
}
