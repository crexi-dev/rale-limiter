using RateLimiter.Configuration;
using RateLimiter.Rules;

namespace RateLimiter
{
    public sealed class RateLimiter : IRateLimiter
    {
        private readonly IRateLimiterConfiguration _configuration;

        public RateLimiter(IRateLimiterConfiguration configuration) 
        {
            _configuration = configuration;
        }

        public bool IsAllowed(string clientToken, string uri)
        {
            var rules = _configuration.GetRules(uri);
            if (rules is null)
                return true;

            foreach (var rule in rules)
            {
                if (!rule.IsAllowed(clientToken, uri))
                    return false;
            }

            return true;
        }
    }
}
