using System.Collections.Generic;
using RateLimiter.Interfaces;

namespace RateLimiter
{
    public class CompositeRateLimitStrategy(IEnumerable<IRateLimitStrategy> strategies) : IRateLimitStrategy
    {
        public bool IsRequestAllowed(string? clientToken)
        {
            foreach (var strategy in strategies)
            {
                if (!strategy.IsRequestAllowed(clientToken)) return false;
            }

            return true;
        }
    }
}
