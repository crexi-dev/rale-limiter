using RateLimiter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Interface
{
    public interface IRateLimiterService
    {
        bool Validate(RequestDTO request);
    }
}
