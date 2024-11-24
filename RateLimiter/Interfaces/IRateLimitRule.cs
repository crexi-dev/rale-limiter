using Crexi.API.Common.RateLimiter.Models;

namespace Crexi.API.Common.RateLimiter.Interfaces
{
    public interface IRateLimitRule
    {
        bool IsRequestAllowed(Client client, string resource);
    }
}
