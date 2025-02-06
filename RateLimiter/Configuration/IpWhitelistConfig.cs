using System.Collections.Generic;

namespace RateLimiter.Configuration
{
    public class IpWhitelistConfig
    {
        public IEnumerable<string> Blocked { get; set; }

        public IpWhitelistConfig()
        {
            Blocked = [];
        }

        public IpWhitelistConfig(IEnumerable<string> blocked)
        {
            Blocked = blocked;
        }
    }
}
