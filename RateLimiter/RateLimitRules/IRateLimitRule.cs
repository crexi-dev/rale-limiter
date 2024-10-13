using System;
using System.Threading.Tasks;

namespace RateLimiterNS.RateLimitRules
{
    public interface IRateLimitRule
    {
         Task<bool> IsRequestAllowedAsync(string token, DateTime requestTime);
    }
}