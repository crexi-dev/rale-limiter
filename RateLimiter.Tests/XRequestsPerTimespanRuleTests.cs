using NUnit.Framework;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RateLimiter.Tests
{
	[TestFixture]
	public class XRequestsPerTimespanRuleTests
	{
		//private TimespanSinceLastCallRule _rule;
		private Dictionary<string, string>? _factors;
		private ConcurrentQueue<RequestLogEntry> _log;

		[SetUp]
		public void Setup()
		{
			//_rule = new(TimeSpan.FromSeconds(0.1));
			_factors = new() { { "k", "v" } };
			_log = new();
			//_rule.CommonLog = _log;
		}


		[Test]
		public void IsRequestAllowed_FirstRequest_ReturnsTrue()
		{
			var rule = new XRequestsPerTimespanRule(5, TimeSpan.FromSeconds(0.1));
			rule.CommonLog = _log;
			var result = rule.IsRequestAllowed("client1", null);
			Assert.IsTrue(result);
		}

		[Test]
		public void IsRequestAllowed_WithinLimit_ReturnsTrue()
		{
			var rule = new XRequestsPerTimespanRule(5, TimeSpan.FromSeconds(0.1));
			rule.CommonLog = _log;
			for (int i = 0; i < 4; i++)
			{
				var isAllowed = rule.IsRequestAllowed("client1", _factors);
				var entry = new RequestLogEntry
				{
					ClientId = "client1",
					Resource = "resource",
					Timestamp = DateTime.UtcNow,
					IsAllowed = isAllowed,
					Factors = new(_factors)
				};

				_log.Enqueue(entry);
			}
			var result = rule.IsRequestAllowed("client1", null);
			Assert.IsTrue(result);
		}

		[Test]
		public void IsRequestAllowed_ExceedsLimit_ReturnsFalse()
		{
			var rule = new XRequestsPerTimespanRule(5, TimeSpan.FromSeconds(0.1));
			rule.CommonLog = _log;
			for (int i = 0; i < 5; i++)
			{
				var isAllowed = rule.IsRequestAllowed("client1", _factors); 
				var entry = new RequestLogEntry
				{
					ClientId = "client1",
					Resource = "resource",
					Timestamp = DateTime.UtcNow,
					IsAllowed = isAllowed,
					Factors = new(_factors)
				};

				_log.Enqueue(entry);
			}
			var result = rule.IsRequestAllowed("client1", null);
			Assert.IsFalse(result);
		}

		[Test]
		public void IsRequestAllowed_AfterTimespan_ReturnsTrue()
		{
			var rule = new XRequestsPerTimespanRule(5, TimeSpan.FromSeconds(0.1));
			rule.CommonLog = _log;
			for (int i = 0; i < 5; i++)
			{
				var isAllowed = rule.IsRequestAllowed("client1", _factors);
				var entry = new RequestLogEntry
				{
					ClientId = "client1",
					Resource = "resource",
					Timestamp = DateTime.UtcNow,
					IsAllowed = isAllowed,
					Factors = new(_factors)
				};

				_log.Enqueue(entry);
			}
			System.Threading.Thread.Sleep(TimeSpan.FromSeconds(0.1));
			var result = rule.IsRequestAllowed("client1", null);
			Assert.IsTrue(result);
		}
	}
}