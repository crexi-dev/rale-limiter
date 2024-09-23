using NUnit.Framework;
using RateLimiter.Interfaces;
using RateLimiter.RateLimiters;
using RateLimiter.Rules;
using System;
using System.Collections.Generic;

namespace RateLimiter.Tests
{
    [TestFixture]
    public class BurstRuleTest
    {
        [Test]
        public void Should_AllowBurstRequests_WithinLimit()
        {
            var rateLimiter = new ResourceRateLimiter(new List<IRateLimitRule>
        {
            new BurstRule(5, TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(1))
        });

            var clientId = "burstClient";
            for (int i = 0; i < 5; i++)
            {
                Assert.IsTrue(rateLimiter.IsRequestAllowed(clientId));
            }
        }

        [Test]
        public void Should_DenyAfterBurstLimitUntilCooldownExpires()
        {
            var rateLimiter = new ResourceRateLimiter(new List<IRateLimitRule>
            {
                new BurstRule(5, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(2)) // Shortened cooldown for testing
            });

                    var clientId = "burstClient";
                    for (int i = 0; i < 5; i++)
                    {
                        Assert.IsTrue(rateLimiter.IsRequestAllowed(clientId));
                    }

                    // 6th request should be denied due to burst limit
                    Assert.IsFalse(rateLimiter.IsRequestAllowed(clientId));

                    // Wait for cooldown period (2 seconds)
                    System.Threading.Thread.Sleep(2100);

                    // After cooldown, requests should be allowed again
                    Assert.IsTrue(rateLimiter.IsRequestAllowed(clientId));
                }
        }

}
