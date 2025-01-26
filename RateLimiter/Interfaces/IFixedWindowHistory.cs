using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Interfaces
{
    public interface IFixedWindowHistory : IHistory
    {
        int GetRequestCount(IIdentifier identifier, DateTime start, DateTime end);

    }
}
