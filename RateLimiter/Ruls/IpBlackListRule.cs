using RateLimiter.Ruls.Abstract;
using RateLimiter.User;
using System;
using System.Linq;

namespace RateLimiter.Ruls
{
    public class IpBlackListRule : RateLimiterRuleDecorator
    {
        private readonly string[] _restrictedIpAddresses;

        public IpBlackListRule(string[] allowedIpAddresses)
        {
            _restrictedIpAddresses = allowedIpAddresses;
        }

        public override bool IsAllowed(IUserData userData)
        {
            return !_restrictedIpAddresses.Contains(userData.IpAddress) && (RateLimiterRule == null || RateLimiterRule.IsAllowed(userData));
        }
    }
}
