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

//[TestFixture]
//public class RateLimiterTest
//{
//	[Test]
//	public void Example()
//	{
//		Assert.That(true, Is.True);
//	}


//	private RateLimiter _rateLimiter;

//	[SetUp]
//	public void SetUp()
//	{
//		_rateLimiter = new RateLimiter();
//	}

//	[Test]
//	public void XRequestsPerTimespanRule_AllowsRequestsWithinLimit()
//	{
//		var rule = new XRequestsPerTimespanRule(5, TimeSpan.FromMinutes(1));
//		_rateLimiter.AddGlobalRule(rule);

//		var factors = new Dictionary<string, string>();

//		for (int i = 0; i < 5; i++)
//		{
//			Assert.IsTrue(_rateLimiter.IsRequestAllowed("resource1", "client1", factors));
//		}

//		Assert.IsFalse(_rateLimiter.IsRequestAllowed("resource1", "client1", factors));
//	}

//	[Test]
//	public void TimespanSinceLastCallRule_AllowsRequestAfterTimespan()
//	{
//		var rule = new TimespanSinceLastCallRule(TimeSpan.FromSeconds(1));
//		_rateLimiter.AddGlobalRule(rule);

//		var factors = new Dictionary<string, string>();

//		Assert.IsTrue(_rateLimiter.IsRequestAllowed("resource1", "client1", factors));
//		Assert.IsFalse(_rateLimiter.IsRequestAllowed("resource1", "client1", factors));

//		System.Threading.Thread.Sleep(1000);

//		Assert.IsTrue(_rateLimiter.IsRequestAllowed("resource1", "client1", factors));
//	}

//	[Test]
//	public void RegionBasedRule_AllowsRequestsBasedOnRegion()
//	{
//		var rule = new RegionBasedRule(5, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(5));
//		_rateLimiter.AddGlobalRule(rule);

//		var usFactors = new Dictionary<string, string> { { "region", "US" } };
//		var euFactors = new Dictionary<string, string> { { "region", "EU" } };

//		for (int i = 0; i < 5; i++)
//		{
//			Assert.IsTrue(_rateLimiter.IsRequestAllowed("resource1", "client1", usFactors));
//		}

//		Assert.IsFalse(_rateLimiter.IsRequestAllowed("resource1", "client1", usFactors));

//		Assert.IsTrue(_rateLimiter.IsRequestAllowed("resource1", "client2", euFactors));
//		Assert.IsFalse(_rateLimiter.IsRequestAllowed("resource1", "client2", euFactors));

//		System.Threading.Thread.Sleep(5000);

//		Assert.IsTrue(_rateLimiter.IsRequestAllowed("resource1", "client2", euFactors));
//	}

//	[Test]
//	public void RateLimiter_LogsRequests()
//	{
//		var rule = new XRequestsPerTimespanRule(5, TimeSpan.FromMinutes(1));
//		_rateLimiter.AddGlobalRule(rule);

//		var factors = new Dictionary<string, string>();

//		for (int i = 0; i < 5; i++)
//		{
//			_rateLimiter.IsRequestAllowed("resource1", "client1", factors);
//		}

//		var log = _rateLimiter.GetRequestLog();

//		Assert.AreEqual(5, log.Count());
//		Assert.IsTrue(log.All(entry => entry.ClientId == "client1" && entry.Resource == "resource1"));
//	}
//}

