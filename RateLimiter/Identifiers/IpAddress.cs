using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RateLimiter.Interfaces;

namespace RateLimiter.Identifiers
{
    public class IpAddress : IIdentifier
    {
        private  string IpAddressValue { get; set; }

        public IpAddress(string ipAddress)
        {
            IpAddressValue = ipAddress;
        }

        public string ToString()
        {
            return IpAddressValue;
        }
    }
}
