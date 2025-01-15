using System;
using FluentAssertions;
using NUnit.Framework;

namespace RateLimiter.Tests;

[TestFixture]
public class RuleTests
{
    [Test]
    public void Rules_Are_Equal_When_Scopes_Are_Same()
    {
        var rule1 = new Rule("foo", 10, TimeSpan.FromDays(1));
        var rule2 = new Rule("foo", 5, TimeSpan.FromSeconds(1));

        rule1.Should().Be(rule2);
    }

    [Test]
    public void Rules_Are_Equal_When_Scopes_Are_Are_Equal_But_Different_Case()
    {
        var rule1 = new Rule("foo", 10, TimeSpan.FromDays(1));
        var rule2 = new Rule("FOO", 5, TimeSpan.FromSeconds(1));

        rule1.Should().Be(rule2);
    }

    [Test]
    public void Rules_Are_Not_Equal_When_Scopes_Are_Different()
    {
        var rule1 = new Rule("foo", 10, TimeSpan.FromDays(1));
        var rule2 = new Rule("bar", 10, TimeSpan.FromDays(1));

        rule1.Should().NotBe(rule2);
    }
}
