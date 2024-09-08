using RateLimiter.Enums;
using RateLimiter.Service.Interface;

namespace RateLimiter.Service
{
    // Rule C:  If US then follow rule A.  If EU then follow rule B.  If neither return false.
    // Region is part of the token.  Normally it would be in the cookie.
    public class RuleC : IRule
    {
        public bool Allow(string resource, string token, DateTime requestTime)
        {
            if (token.Contains(RegionEnum.US.ToString()))
            { 
                var ruleA = new RuleA();
                return ruleA.Allow(resource, token, requestTime);
            }
            else if (token.Contains(RegionEnum.EU.ToString()))
            {
                var ruleB = new RuleB();
                return ruleB.Allow(resource, token, requestTime);
            }

            return false;
        }
    }
}