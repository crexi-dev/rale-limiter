using RateLimiter.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RateLimiter.Components.RuleService
{
    public interface IRateLimitingService
    {
        Task<bool> CanProcessRequestAsync(RateLimitingRequestData requestData, List<string> groups);
    }
}
