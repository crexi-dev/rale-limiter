using System;
using System.Threading.Tasks;

namespace RateLimiter;

public interface IRateLimitRule
{
    Task<bool> IsRequestAllowedAsync(string token);
    TimeSpan GetTimeUntilReset(string token);
}
