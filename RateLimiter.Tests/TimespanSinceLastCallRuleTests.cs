using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Resources;

using NUnit.Framework;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RateLimiter.Tests
{
	[TestFixture]
	public class TimespanSinceLastCallRuleTests
	{
		private TimespanSinceLastCallRule _rule;
		private Dictionary<string, string>? _factors;
		private ConcurrentQueue<RequestLogEntry> _log;

		[SetUp]
		public void Setup()
		{
			_rule = new(TimeSpan.FromSeconds(0.1));
			_factors = new() { {"k", "v" } };
			_log = new();
			_rule.CommonLog = _log;
		}

		[Test]
		public void IsRequestAllowed_FirstRequest_ReturnsTrue()
		{
			var result = _rule.IsRequestAllowed("client1", _factors);
			Assert.IsTrue(result);
		}

		[Test]
		public void IsRequestAllowed_RequestWithinTimespan_ReturnsFalse()
		{
			var isAllowed = _rule.IsRequestAllowed("client1", _factors); // First request
			System.Threading.Thread.Sleep(10); // Wait for 0.01 seconds

			var entry = new RequestLogEntry
			{
				ClientId = "client1",
				Resource = "resource",
				Timestamp = DateTime.UtcNow,
				IsAllowed = isAllowed,
				Factors = new(_factors)
			};

			_log.Enqueue(entry);



			var result = _rule.IsRequestAllowed("client1", _factors);
			Assert.IsFalse(result);
		}

		[Test]
		public void IsRequestAllowed_RequestAfterTimespan_ReturnsTrue()
		{
			



			_rule.IsRequestAllowed("client1", _factors); // First request
			System.Threading.Thread.Sleep(110); // Wait for 0.11 seconds

			var result = _rule.IsRequestAllowed("client1", _factors);
			Assert.IsTrue(result);
		}
	}
}