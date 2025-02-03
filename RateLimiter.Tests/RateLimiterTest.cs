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
        private Mock<ILogger<RateLimiter>> _loggerMock;

        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<RateLimiter>>();
        }

        [Test]
        public void IsRequestAllowed_NoRules_ReturnsTrue()
        {
            var limiter = new RateLimiter(_loggerMock.Object);
            Assert.That(limiter.IsRequestAllowed("client1", "res1"), Is.True);
        }

        [Test]
        public void IsRequestAllowed_AllRulesAllow_ReturnsTrue()
        {
            var ruleMock = new Mock<IRateLimitRule>();
            ruleMock.Setup(r => r.IsAllowed("client1", "res1")).Returns(true);

            var limiter = new RateLimiter(_loggerMock.Object);
            limiter.AddRule("res1", ruleMock.Object);

            Assert.That(limiter.IsRequestAllowed("client1", "res1"), Is.True);
        }

        [Test]
        public void IsRequestAllowed_AnyRuleDenies_ReturnsFalse()
        {
            var rule1 = new Mock<IRateLimitRule>();
            rule1.Setup(r => r.IsAllowed("client1", "res1")).Returns(true);

            var rule2 = new Mock<IRateLimitRule>();
            rule2.Setup(r => r.IsAllowed("client1", "res1")).Returns(false);

            var limiter = new RateLimiter(_loggerMock.Object);
            limiter.AddRule("res1", rule1.Object);
            limiter.AddRule("res1", rule2.Object);

            Assert.That(limiter.IsRequestAllowed("client1", "res1"), Is.False);
        }

        [Test]
        public void Cleanup_CallsAllRuleCleanups()
        {
            var ruleMock = new Mock<IRateLimitRule>();
            var limiter = new RateLimiter(_loggerMock.Object);
            limiter.AddRule("res1", ruleMock.Object);

            limiter.Cleanup();

            ruleMock.Verify(r => r.Cleanup(), Times.Once);
        }

        [Test]
        public void WhenRequestBlocked_LogsWarning()
        {
            var rule = new FixedWindowRule(0, TimeSpan.FromMinutes(1));
            var limiter = new RateLimiter(_loggerMock.Object);
            limiter.AddRule("res1", rule);

            limiter.IsRequestAllowed("client1", "res1");

            _loggerMock.Verify(log => log.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Request blocked: Client client1, Resource res1")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
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