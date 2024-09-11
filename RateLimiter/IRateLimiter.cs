using RateLimiter.Rules;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RateLimiter;

public interface IRateLimiter
{
    Task<bool> IsRequestAllowed(string clientId, string resource);
}