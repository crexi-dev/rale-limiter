using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using RateLimiter.Models;
using RateLimiter.Rules;
using RateLimiter.Interfaces;
using RateLimiter.Builder;

namespace RateLimiter.Tests;

/// <summary>
/// Collection of tests to demonstrate RateLimiter project usage. 
/// Given more time, I would provide more detailed tests of each class and aim for 100% code coverage.
/// </summary>
[TestFixture]
public class RateLimiterTests
{
	[Test]
	public void RateLimitRuleBuilder_CombinesTwoPassingRulesWithAnd_ReturnsAllowed()
	{
		// Arrange
		var rule1 = new MaxRequestsPerTimeSpanRule(5, TimeSpan.FromSeconds(10));
		var rule2 = new MinimumTimeBetweenRequestsRule(TimeSpan.FromSeconds(2));

		// Act
		var combinedRule = new RateLimitRuleBuilder()
			.Add(rule1)
			.Add(rule2)
			.Build();

		var context = new RateLimitContext("client-123", "api/resource", Region.US);
		var result = combinedRule.Evaluate(context);

		// Assert
		Assert.IsTrue(result.Allowed);
		Assert.IsEmpty(result.RejectedReasons);
	}

	[Test]
	public void RateLimitRuleBuilder_CombinesTwoRulesWithOr_ReturnsAllowed()
	{
		// Arrange
		var rule1 = new MaxRequestsPerTimeSpanRule(1, TimeSpan.FromSeconds(10));
		var rule2 = new MinimumTimeBetweenRequestsRule(TimeSpan.FromSeconds(5));
		var builder = new RateLimitRuleBuilder();

		// Act, Assert
		var combinedRule = builder
			.Or(new List<IRateLimitRule> { rule1, rule2 })
			.Build();
		var context = new RateLimitContext("client-123", "api/resource", Region.US);

		var response1 = combinedRule.Evaluate(context);
		Assert.IsTrue(response1.Allowed);

		var response2 = combinedRule.Evaluate(context);
		Assert.IsFalse(response2.Allowed);
		Thread.Sleep(6000);

		var response3 = combinedRule.Evaluate(context);
		Assert.IsTrue(response3.Allowed);
	}

	[Test]
	public void RateLimitRuleBuilder_RegionMatchesAndEvaluatesRule_ReturnsAllowed()
	{
		// Arrange
		var rule = new MaxRequestsPerTimeSpanRule(1, TimeSpan.FromSeconds(10));
		var builder = new RateLimitRuleBuilder();
		var region = Region.US;

		// Act
		var combinedRule = builder
			.AddForRegion(rule, region)
			.Build();
		var context = new RateLimitContext("client-123", "api/resource", region);

		// Assert
		var response = combinedRule.Evaluate(context);
		Assert.IsTrue(response.Allowed);
	}

	[Test]
	public void RateLimitRuleBuilder_RegionBasedRules_ReturnsAllowedForUSAndNotAllowedForEU()
	{
		// Arrange
		var usRule = new MaxRequestsPerTimeSpanRule(5, TimeSpan.FromSeconds(10));
		var euRule = new MinimumTimeBetweenRequestsRule(TimeSpan.FromSeconds(5));
		var builder = new RateLimitRuleBuilder();

		// Act
		var combinedRule = builder
			.AddForRegion(usRule, Region.US)
			.AddForRegion(euRule, Region.EU)
			.Build();

		// US-based context
		var usContext = new RateLimitContext("us-client-123", "api/resource", Region.US);
		for (int i = 0; i < 5; i++)
		{
			var response = combinedRule.Evaluate(usContext);
			Assert.IsTrue(response.Allowed);
		}

		// EU-based context
		var euContext = new RateLimitContext("eu-client-456", "api/resource", Region.EU);
		var response1 = combinedRule.Evaluate(euContext);
		Assert.IsTrue(response1.Allowed);

		var response2 = combinedRule.Evaluate(euContext);
		Assert.IsFalse(response2.Allowed);
		Assert.That(response2.RejectedReasons, Has.Exactly(1).EqualTo(nameof(MinimumTimeBetweenRequestsRule)));
	}
}
