using RateLimiter.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Services
{
    public interface IRateLimiterService
    {
        Task<bool> AllowAPIRequest(AccessToken token);
    }
}
