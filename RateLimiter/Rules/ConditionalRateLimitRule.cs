using Crexi.API.Common.RateLimiter.Interfaces;
using Crexi.API.Common.RateLimiter.Models;
using System;

namespace Crexi.API.Common.RateLimiter.Rules
{

    public class ConditionalRateLimitRule : IRateLimitRule
    {
        private readonly Func<Client, bool> _condition;
        private readonly IRateLimitRule _rule;

        public ConditionalRateLimitRule(Func<Client, bool> condition, IRateLimitRule rule)
        {
            _condition = condition;
            _rule = rule;
        }

        public bool IsRequestAllowed(Client client, string resource)
        {
            if (_condition(client))
            {
                return _rule.IsRequestAllowed(client, resource);
            }
            return true;
        }
    }
}
