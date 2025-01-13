﻿using System;
using NUnit.Framework;

namespace RateLimiter.Tests
{
    [TestFixture]
    public class FixedWindowRuleTests
    {
        [Test]
        public void FirstRequest_Should_BeAllowed()
        {
            var usageRepo = new InMemoryUsageRepository();

            // assume we only allow 1 request in a 1-minute window for this test
            var rule = new FixedWindowRule(limit: 1, window: TimeSpan.FromMinutes(1), usageRepo);
            var isAllowed = rule.IsRequestAllowed("crexi-client123");

            // assert
            Assert.IsTrue(isAllowed, "first request from a new client should always be allowed.");

        }

        [Test]
        public void SecondRequest_WithinSameWindow_Should_BeBlocked()
        {

            throw new NotImplementedException();
        }
    }
}
