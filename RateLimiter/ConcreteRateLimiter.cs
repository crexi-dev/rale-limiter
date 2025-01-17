using RateLimiter.Ruls.Abstract;
using RateLimiter.User;

namespace RateLimiter
{
    public class ConcreteRateLimiter
    {
        private readonly RateLimiterRuleDecorator? _rateLimiter;
        public ConcreteRateLimiter(RateLimiterRuleDecorator[] rules)
        {
            foreach (var rule in rules)
            {
                if (_rateLimiter == null)
                {
                    _rateLimiter = rule;
                }
                else
                {
                    rule.RateLimiterRule = _rateLimiter;
                    _rateLimiter = rule;
                }
            }
        }
        public bool IsAllowed(IUserData userData)
        {
            return _rateLimiter == null || _rateLimiter.IsAllowed(userData);
        }

    }
}
