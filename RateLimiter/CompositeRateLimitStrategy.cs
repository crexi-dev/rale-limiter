using System.Collections.Generic;
using RateLimiter.Interfaces;

namespace RateLimiter
{
    internal class CompositeRateLimitStrategy : IRateLimitStrategy
    {
        private readonly IEnumerable<IRateLimitStrategy> _strategies;

        public CompositeRateLimitStrategy(IEnumerable<IRateLimitStrategy> strategies) 
        {
            _strategies = strategies;
        }

        public bool IsRequestAllowed(string clientToken)
        {
            foreach (var strategy in _strategies)
            {
                if (!strategy.IsRequestAllowed(clientToken)) return false;
            }

            return true;
        }
    }
}
