using RateLimiter.Ruls.Abstract;
using RateLimiter.User;
using System;
using System.Linq;

namespace RateLimiter.Ruls
{
    public class IpWhiteListRule : RateLimiterRuleDecorator
    {
        private string[] _allowedIpAddresses;

        public IpWhiteListRule(string[] allowedIpAddresses)
        {
            _allowedIpAddresses = allowedIpAddresses;
        }

        public override bool IsAllowed(IUserData userData)
        {
            return _allowedIpAddresses.Contains(userData.IpAddress) && (RateLimiterRule == null || RateLimiterRule.IsAllowed(userData));
        }
    }
}
