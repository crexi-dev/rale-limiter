using RateLimiter.User;

namespace RateLimiter.Ruls.Abstract
{
    public abstract class RateLimiterRuleDecorator
    {
        public RateLimiterRuleDecorator? RateLimiterRule { get; set; }
        public abstract bool IsAllowed(IUserData userData);
    }
}
