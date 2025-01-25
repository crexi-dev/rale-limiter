using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RateLimiter.Interfaces;

namespace RateLimiter.Identifiers
{
    sealed class IpAddress : IIdentifier
    {
        private readonly string IpAddress { get; set; }

        public IpAddress(string ipAddress)
        {
            IpAddress = ipAddress;
        }

        public string ToString()
        {
            return ipAddress;
        }
    }
}
