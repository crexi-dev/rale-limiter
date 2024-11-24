using Crexi.API.Common.RateLimiter.Models;

namespace Crexi.API.Common.RateLimiter.Interfaces
{
    public interface IRateLimiter
    {
        bool IsRequestAllowed(Client client, string resource);

        void ConfigureResource(string resource, IRateLimitRule rule);
    }
}
