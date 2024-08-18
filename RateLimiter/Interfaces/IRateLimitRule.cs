using System;
using System.Threading.Tasks;
using RateLimiter.Enums;

namespace RateLimiter.Interfaces
{
    public interface IRateLimitRule
    {
        Task<bool> IsRequestAllowedAsync(string accessToken, DateTime requestTime, Region region);
        Task RecordRequest(string accessToken, DateTime requestTime, Region region);
    }
}
