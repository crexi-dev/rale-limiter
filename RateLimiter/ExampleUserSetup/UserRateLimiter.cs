using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.ExampleUserSetup
{
    public class UsersRateLimiter
    {
        private IRateLimiter RateLimiter { get; }
        public UsersRateLimiter(IRateLimiter rateLimiter)
        {
            RateLimiter = rateLimiter;
        }
        public bool CheckRequest(IUser user, string route)
        {
            return RateLimiter.CheckRequest($"{user.Name}:{route}");
        }


    }
}
