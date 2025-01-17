using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.User
{
    public interface IUserData
    {
        string? IpAddress { get; set; }
        string? CountryCode { get; set; }
        string? Token { get; set; }
    }
}
