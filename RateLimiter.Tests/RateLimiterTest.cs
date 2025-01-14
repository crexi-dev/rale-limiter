using System;
using FluentAssertions;
using NUnit.Framework;

namespace RateLimiter.Tests;

[TestFixture]
public class RateLimiterTest
{
	[Test]
	public void Example()
    {
        true.Should().BeTrue();
    }

    [Test]
    public void Rule_Test()
    {
        var rule = new Rule("foo", 10, TimeSpan.FromDays(1));
    }
}
