using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Interfaces
{
    interface IHistory
    {
        void Record(IIdentifier identifier, DateTime now);
    }
}
