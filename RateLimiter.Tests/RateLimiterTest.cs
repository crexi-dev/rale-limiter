﻿using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using NUnit.Framework;
using RateLimiter.Rules;
using System;

namespace RateLimiter.Tests;

[TestFixture]
public class RateLimiterTest
{
	[Test]
	public void FixedWindowLimiterRule_should_not_allow_excessive_requests()
	{
        var fakeTime = new FakeTimeProvider(startDateTime: DateTimeOffset.UtcNow);
        var rateLimiter = new RateLimiter();
		rateLimiter.AddRule("api", new FixedWindowLimiterRule(fakeTime).Configure(new FixedWindowLimiterRuleConfiguration(Window: TimeSpan.FromSeconds(5), Limit: 2)));

		rateLimiter.IsRequestAllowed("api", "user1").Should().BeTrue();
        rateLimiter.IsRequestAllowed("api", "user1").Should().BeTrue();
        rateLimiter.IsRequestAllowed("api", "user1").Should().BeFalse();

        fakeTime.Advance(TimeSpan.FromSeconds(5));

        rateLimiter.IsRequestAllowed("api", "user1").Should().BeTrue();
    }

    [Test]
    public void FixedWindowLimiterRule_should_have_separate_limits_for_different_users()
    {
        var fakeTime = new FakeTimeProvider(startDateTime: DateTimeOffset.UtcNow);
        var rateLimiter = new RateLimiter();
        rateLimiter.AddRule("api", new FixedWindowLimiterRule(fakeTime).Configure(new FixedWindowLimiterRuleConfiguration(Window: TimeSpan.FromSeconds(5), Limit: 1)));

        rateLimiter.IsRequestAllowed("api", "user1").Should().BeTrue();
        rateLimiter.IsRequestAllowed("api", "user1").Should().BeFalse();

        rateLimiter.IsRequestAllowed("api", "user2").Should().BeTrue();
        rateLimiter.IsRequestAllowed("api", "user2").Should().BeFalse();
    }

    [Test]
    public void Each_resource_should_have_own_rule()
    {
        var fakeTime = new FakeTimeProvider(startDateTime: DateTimeOffset.UtcNow);
        var rateLimiter = new RateLimiter();
        rateLimiter.AddRule("api/resource1", new FixedWindowLimiterRule(fakeTime).Configure(new FixedWindowLimiterRuleConfiguration(Window: TimeSpan.FromSeconds(5), Limit: 1)));
        rateLimiter.AddRule("api/resource2", new FixedWindowLimiterRule(fakeTime).Configure(new FixedWindowLimiterRuleConfiguration(Window: TimeSpan.FromSeconds(5), Limit: 1)));

        rateLimiter.IsRequestAllowed("api/resource1", "user1").Should().BeTrue();
        rateLimiter.IsRequestAllowed("api/resource1", "user1").Should().BeFalse();

        rateLimiter.IsRequestAllowed("api/resource2", "user1").Should().BeTrue();
        rateLimiter.IsRequestAllowed("api/resource2", "user1").Should().BeFalse();
    }

    [Test]
    public void SingleRequestPerWindowLimiterRule_should_allow_only_one_request_per_timespan()
    {
        var fakeTime = new FakeTimeProvider(startDateTime: DateTimeOffset.UtcNow);
        var rateLimiter = new RateLimiter();
        rateLimiter.AddRule("api", new SingleRequestPerWindowLimiterRule(fakeTime).Configure(TimeSpan.FromSeconds(5)));

        rateLimiter.IsRequestAllowed("api", "user1").Should().BeTrue();
        rateLimiter.IsRequestAllowed("api", "user1").Should().BeFalse();

        rateLimiter.IsRequestAllowed("api", "user2").Should().BeTrue();
        rateLimiter.IsRequestAllowed("api", "user2").Should().BeFalse();
    }

    [Test]
    public void RateLimiter_shouldnt_apply_any_rules_to_not_configured_resources()
    {
        var fakeTime = new FakeTimeProvider(startDateTime: DateTimeOffset.UtcNow);
        var rateLimiter = new RateLimiter();
        rateLimiter.AddRule("api/resource1", new SingleRequestPerWindowLimiterRule(fakeTime).Configure(TimeSpan.FromSeconds(5)));

        rateLimiter.IsRequestAllowed("api/resource2", "user1").Should().BeTrue();
        rateLimiter.IsRequestAllowed("api/resource2", "user1").Should().BeTrue();
    }

    [Test]
    public void Combination_of_rules_should_be_supported_for_resource()
    {
        var fakeTime = new FakeTimeProvider(startDateTime: DateTimeOffset.UtcNow);
        var rateLimiter = new RateLimiter();
        rateLimiter.AddRule("api", new FixedWindowLimiterRule(fakeTime).Configure(new FixedWindowLimiterRuleConfiguration(Window: TimeSpan.FromSeconds(5), Limit: 3)));
        rateLimiter.AddRule("api", new SingleRequestPerWindowLimiterRule(fakeTime).Configure(TimeSpan.FromSeconds(1)));

        rateLimiter.IsRequestAllowed("api", "user1").Should().BeTrue();
        rateLimiter.IsRequestAllowed("api", "user1").Should().BeFalse(); //should be blocked by SingleRequestPerWindowLimiterRule, 1 second haven't passed yet

        fakeTime.Advance(TimeSpan.FromSeconds(1));
        rateLimiter.IsRequestAllowed("api", "user1").Should().BeTrue();

        fakeTime.Advance(TimeSpan.FromSeconds(1));
        rateLimiter.IsRequestAllowed("api", "user1").Should().BeFalse(); //should be blocked by FixedWindowLimiterRule because 3 previuos requests in 5 second time span were already allowed by this rule
    }
}