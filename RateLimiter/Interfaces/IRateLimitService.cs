using RateLimiter.Models.Apis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Interfaces
{
    /// <summary>
    /// Interface for rate limit checking service.
    /// </summary>
    public interface IRateLimitService
    {
        Task<RateLimiteResponse> CheckRateLimitAsync(RateLimitRequest request);
    }
}
