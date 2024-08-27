
using NUnit.Framework;

namespace RateLimiter.Tests
{
	public static class RateLimitRuleTestHelper
	{
        public static void VerifyRateLimitRuleAllowedDenied(IRateLimitRule rateLimitRule, long allowed, long denied)
        {
            Assert.That(rateLimitRule.Allowed, Is.EqualTo(allowed));
            Assert.That(rateLimitRule.Denied, Is.EqualTo(denied));
        }
    }
}

