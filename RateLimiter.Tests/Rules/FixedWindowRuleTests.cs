using FluentAssertions;

using RateLimiter.Rules;

using System;
using System.Threading;
using Xunit;

namespace RateLimiter.Tests.Rules
{
    public class FixedWindowRuleTests
    {
        [Fact]
        public void WhenFoo_DoesBar()
        {
            var rule = new FixedWindowRule(3, TimeSpan.FromSeconds(3));

            // First 3 requests allowed
            rule.IsAllowed("client1").Should().BeTrue();
            rule.IsAllowed("client1").Should().BeTrue();
            rule.IsAllowed("client1").Should().BeTrue();

            // Fourth request blocked
            rule.IsAllowed("client1").Should().BeFalse();

            // wait 3 seconds ...
            Thread.Sleep(3000);

            // First 3 requests allowed
            rule.IsAllowed("client1").Should().BeTrue();
            rule.IsAllowed("client1").Should().BeTrue();
            rule.IsAllowed("client1").Should().BeTrue();

            // Fourth request blocked
            rule.IsAllowed("client1").Should().BeFalse();
        }
    }
}
