using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Microsoft.Extensions.Logging;
using RateLimiter.Rules;

namespace RateLimiter.Tests
{
    [TestFixture]
    public class FixedWindowRuleTests
    {
        [Test]
        public void IsAllowed_WithinLimit_ReturnsTrue()
        {
            var rule = new FixedWindowRule(2, TimeSpan.FromMinutes(1));
            Assert.That(rule.IsAllowed("client1", "res1"), Is.True);
            Assert.That(rule.IsAllowed("client1", "res1"), Is.True);
        }

        [Test]
        public void IsAllowed_ExceedsLimit_ReturnsFalse()
        {
            var rule = new FixedWindowRule(2, TimeSpan.FromMinutes(1));
            rule.IsAllowed("client1", "res1");
            rule.IsAllowed("client1", "res1");
            Assert.That(rule.IsAllowed("client1", "res1"), Is.False);
        }

        [Test]
        public void IsAllowed_AfterWindowReset_AllowsAgain()
        {
            var rule = new FixedWindowRule(1, TimeSpan.FromMilliseconds(50));
            rule.IsAllowed("client1", "res1");
            Thread.Sleep(100);
            Assert.That(rule.IsAllowed("client1", "res1"), Is.True);
        }

        [Test]
        public void Cleanup_RemovesExpiredEntries()
        {
            var rule = new FixedWindowRule(1, TimeSpan.FromMilliseconds(50));
            rule.IsAllowed("client1", "res1");
            Thread.Sleep(100);
            rule.Cleanup();
            Assert.That(rule.IsAllowed("client1", "res1"), Is.True);
        }
    }

    [TestFixture]
    public class SlidingWindowRuleTests
    {
        [Test]
        public void IsAllowed_WithinLimit_ReturnsTrue()
        {
            var rule = new SlidingWindowRule(3, TimeSpan.FromMinutes(1));
            Assert.That(rule.IsAllowed("client1", "res1"), Is.True);
            Assert.That(rule.IsAllowed("client1", "res1"), Is.True);
            Assert.That(rule.IsAllowed("client1", "res1"), Is.True);
        }

        [Test]
        public void IsAllowed_ExceedsLimit_ReturnsFalse()
        {
            var rule = new SlidingWindowRule(2, TimeSpan.FromMinutes(1));
            rule.IsAllowed("client1", "res1");
            rule.IsAllowed("client1", "res1");
            Assert.That(rule.IsAllowed("client1", "res1"), Is.False);
        }

        [Test]
        public void IsAllowed_AfterSlidingWindow_AllowsAgain()
        {
            var rule = new SlidingWindowRule(2, TimeSpan.FromMilliseconds(100));
            rule.IsAllowed("client1", "res1");
            Thread.Sleep(50);
            rule.IsAllowed("client1", "res1");
            Thread.Sleep(60);
            Assert.That(rule.IsAllowed("client1", "res1"), Is.True);
        }

        [Test]
        public void Cleanup_RemovesOldRequests()
        {
            var rule = new SlidingWindowRule(2, TimeSpan.FromMilliseconds(50));
            rule.IsAllowed("client1", "res1");
            Thread.Sleep(100);
            rule.Cleanup();
            Assert.That(rule.IsAllowed("client1", "res1"), Is.True);
        }
    }

    [TestFixture]
    public class RegionalRuleTests
    {
        [Test]
        public void IsAllowed_UsesCorrectRegionRule()
        {
            var usRule = new Mock<IRateLimitRule>();
            usRule.Setup(r => r.IsAllowed("US-123", "res1")).Returns(true);

            var euRule = new Mock<IRateLimitRule>();
            euRule.Setup(r => r.IsAllowed("EU-456", "res1")).Returns(true);

            var rule = new RegionalRule(new Dictionary<string, IRateLimitRule>
            {
                ["US"] = usRule.Object,
                ["EU"] = euRule.Object
            });

            rule.IsAllowed("US-123", "res1");
            usRule.Verify(r => r.IsAllowed("US-123", "res1"), Times.Once);

            rule.IsAllowed("EU-456", "res1");
            euRule.Verify(r => r.IsAllowed("EU-456", "res1"), Times.Once);
        }

        [Test]
        public void IsAllowed_UnknownRegion_ReturnsFalse()
        {
            var rule = new RegionalRule(new Dictionary<string, IRateLimitRule>());
            Assert.That(rule.IsAllowed("ASIA-789", "res1"), Is.False);
        }
    }

    [TestFixture]
    public class RateLimiterTests
    {
        private RateLimiter _rateLimiter;
        private Mock<ILogger<RateLimiter>> _loggerMock;

        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<RateLimiter>>();
            _rateLimiter = new RateLimiter(_loggerMock.Object);
        }

        [Test]
        public void ResourceWithSingleFixedWindowRule_ShouldAllowWithinLimit()
        {
            var rule = new FixedWindowRule(3, TimeSpan.FromSeconds(10));
            _rateLimiter.AddRule("resourceA", rule);

            for (int i = 0; i < 3; i++)
            {
                Assert.IsTrue(_rateLimiter.IsRequestAllowed("client1", "resourceA"));
            }
            Assert.IsFalse(_rateLimiter.IsRequestAllowed("client1", "resourceA"));
        }

        [Test]
        public void ResourceWithSingleSlidingWindowRule_ShouldAllowWithinLimit()
        {
            var rule = new SlidingWindowRule(2, TimeSpan.FromSeconds(5));
            _rateLimiter.AddRule("resourceB", rule);

            Assert.IsTrue(_rateLimiter.IsRequestAllowed("client2", "resourceB"));
            Assert.IsTrue(_rateLimiter.IsRequestAllowed("client2", "resourceB"));
            Assert.IsFalse(_rateLimiter.IsRequestAllowed("client2", "resourceB"));
        }

        [Test]
        public void ResourceWithMultipleRules_ShouldDenyIfAnyRuleFails()
        {
            var fixedRule = new FixedWindowRule(2, TimeSpan.FromSeconds(10));
            var slidingRule = new SlidingWindowRule(3, TimeSpan.FromSeconds(5));

            _rateLimiter.AddRule("resourceC", fixedRule);
            _rateLimiter.AddRule("resourceC", slidingRule);

            Assert.IsTrue(_rateLimiter.IsRequestAllowed("client3", "resourceC"));
            Assert.IsTrue(_rateLimiter.IsRequestAllowed("client3", "resourceC"));

            Assert.IsFalse(_rateLimiter.IsRequestAllowed("client3", "resourceC"));

            Thread.Sleep(10000);
            Assert.IsTrue(_rateLimiter.IsRequestAllowed("client3", "resourceC"));
        }

        [Test]
        public void RegionalRule_ShouldApplyCorrectRegionalLimits()
        {
            var regionalRule = new RegionalRule(new Dictionary<string, IRateLimitRule>
            {
                { "US", new FixedWindowRule(1, TimeSpan.FromSeconds(10)) },
                { "ASIA", new SlidingWindowRule(2, TimeSpan.FromSeconds(5)) }
            });

            _rateLimiter.AddRule("resourceD", regionalRule);

            Assert.IsTrue(_rateLimiter.IsRequestAllowed("US-client", "resourceD"));
            Assert.IsFalse(_rateLimiter.IsRequestAllowed("US-client", "resourceD"));

            Assert.IsTrue(_rateLimiter.IsRequestAllowed("ASIA-client", "resourceD"));
            Assert.IsTrue(_rateLimiter.IsRequestAllowed("ASIA-client", "resourceD"));
            Assert.IsFalse(_rateLimiter.IsRequestAllowed("ASIA-client", "resourceD"));
        }
    }

    [TestFixture]
    public class ConcurrencyTests
    {
        [Test]
        public async Task FixedWindowRule_WithConcurrentRequests_EnforcesLimit()
        {
            var rule = new FixedWindowRule(10, TimeSpan.FromSeconds(1));
            var tasks = new List<Task>();

            for (int i = 0; i < 15; i++)
            {
                tasks.Add(Task.Run(() => rule.IsAllowed("client1", "res1")));
            }

            await Task.WhenAll(tasks);

            Assert.That(rule.IsAllowed("client1", "res1"), Is.False);
        }
    }
}