using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Interfaces
{
    public interface ITimespanHistory : IHistory
    {
        DateTime GetLastRequestDate(IIdentifier identifier);
    }
}
