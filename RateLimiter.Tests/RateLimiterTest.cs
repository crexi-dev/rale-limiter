using System.Collections.Generic;
using System.Linq;
using System;

using NUnit.Framework;
using System.Collections.Concurrent;
namespace RateLimiter.Tests;

[TestFixture]
public class RateLimiterTests
{
		private RateLimiter _rateLimiter;

		[SetUp]
		public void Setup()
		{
			_rateLimiter = new RateLimiter();
		}

		[Test]
		public void IsRequestAllowed_ShouldReturnTrue_WhenAllRulesAllow()
		{
			// Arrange
			var rule = new MockRateLimitingRule { IsAllowed = true };
			_rateLimiter.AddGlobalRule(rule);

			// Act
			var result = _rateLimiter.IsRequestAllowed("resource1", "client1", null);

			// Assert
			Assert.IsTrue(result);
		}

		[Test]
		public void IsRequestAllowed_ShouldReturnFalse_WhenAnyRuleDisallows()
		{
			// Arrange
			var rule = new MockRateLimitingRule { IsAllowed = false };
			_rateLimiter.AddGlobalRule(rule);

			// Act
			var result = _rateLimiter.IsRequestAllowed("resource1", "client1", null);

			// Assert
			Assert.IsFalse(result);
		}

		[Test]
		public void GetRequestLog_ShouldReturnLogEntries()
		{
			// Arrange
			var rule = new MockRateLimitingRule { IsAllowed = true };
			_rateLimiter.AddGlobalRule(rule);
			_rateLimiter.IsRequestAllowed("resource1", "client1", null);

			// Act
			var log = _rateLimiter.GetRequestLog();

			// Assert
			Assert.AreEqual(1, log.Count());
		}
	}

	public class MockRateLimitingRule : BaseRule
	{
		public bool IsAllowed { get; set; }
		public ConcurrentQueue<RequestLogEntry> CommonLog { get; set; }

		public override bool IsRequestAllowed(string clientId, Dictionary<string, string>? factors)
		{
			return IsAllowed;
		}
	}



