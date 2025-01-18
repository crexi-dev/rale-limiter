using RateLimiter.Models;

namespace RateLimiter.Interfaces
{
    public interface IRateLimiter
    {
        void Configure(string resource, RuleSet ruleSet);

        bool IsAllowed(string clientId, string resource);
    }
}
