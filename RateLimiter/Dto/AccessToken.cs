using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Dto
{
    public class AccessToken
    {
        public string EndpointUrl { get; set; }
        public string SessionId { get; set; }
    }
}
