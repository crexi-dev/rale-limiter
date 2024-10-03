using NUnit.Framework;
using System;
using System.Threading;
using RateLimiter;

namespace RateLimiter.Tests
{
	[TestFixture]
	public class RateLimiterTest
	{
		private RateLimiterManager _rateLimiterManager = null!;

		[SetUp]
		public void Setup()
		{
			_rateLimiterManager = new RateLimiterManager();
		}

		[Test]
		public void TestFixedWindowRateLimiting()
		{
			Assert.NotNull(_rateLimiterManager);

			var fixedWindowRule = new FixedWindowRateLimitRule(5, TimeSpan.FromMinutes(1));
			_rateLimiterManager.AddRule("resourceA", fixedWindowRule);

			var clientId = "client1";
			var resource = "resourceA";
			var region = "US";
			var token = "token123";

			// Should be allowed up to 5 times
			for (int i = 0; i < 5; i++)
			{
				Assert.That(_rateLimiterManager.IsRequestAllowed(clientId, resource, region, token), Is.True);
			}

			// 6th request should be denied
			Assert.That(_rateLimiterManager.IsRequestAllowed(clientId, resource, region, token), Is.False);
		}

		[Test]
		public void TestFixedWindowRateLimiting_WithWait()
		{
			Assert.NotNull(_rateLimiterManager);

			// Create a rule that allows up to 5 requests in a 15-second window
			var fixedWindowRule = new FixedWindowRateLimitRule(5, TimeSpan.FromSeconds(5));
			_rateLimiterManager.AddRule("resourceA", fixedWindowRule);

			var clientId = "client1";
			var resource = "resourceA";
			var region = "US";
			var token = "token123";

			// Make 2 requests; they should both be allowed
			for (int i = 0; i < 2; i++)
			{
				Assert.That(_rateLimiterManager.IsRequestAllowed(clientId, resource, region, token), Is.True);
			}

			// Wait for 15 seconds to reset the time window
			Thread.Sleep(5000);

			// Make up to 5 more requests; they should all be allowed
			for (int i = 0; i < 5; i++)
			{
				Assert.That(_rateLimiterManager.IsRequestAllowed(clientId, resource, region, token), Is.True);
			}
		}

		[Test]
		public void TestTokenRateLimiting()
		{
			var tokenRule = new TokenRateLimitRule(5, TimeSpan.FromMinutes(1));
			_rateLimiterManager.AddRule("resourceA", tokenRule);

			var token = "abc123";
			var resource = "resourceA";
			var region = "US";

			// Allow up to 5 requests
			for (int i = 0; i < 5; i++)
			{
				Assert.That(_rateLimiterManager.IsRequestAllowed("client1", resource, region, token), Is.True);
			}

			// 6th request should be denied
			Assert.That(_rateLimiterManager.IsRequestAllowed("client1", resource, region, token), Is.False);
		}

		[Test]
		public void TestSlidingWindowRateLimiting()
		{
			var slidingWindowRule = new SlidingWindowRateLimitRule(5, TimeSpan.FromSeconds(5));
			_rateLimiterManager.AddRule("resourceA", slidingWindowRule);

			var clientId = "client1";
			var resource = "resourceA";
			var region = "US";
			var token = "token123";

			// Allow up to 5 requests
			for (int i = 0; i < 5; i++)
			{
				Assert.That(_rateLimiterManager.IsRequestAllowed(clientId, resource, region, token), Is.True);
			}

			// 6th request should be denied
			Assert.That(_rateLimiterManager.IsRequestAllowed(clientId, resource, region, token), Is.False);

			// Wait 15 seconds for window to reset
			Thread.Sleep(5000);

			// Requests should now be allowed again
			Assert.That(_rateLimiterManager.IsRequestAllowed(clientId, resource, region, token), Is.True);
		}

		[Test]
		public void TestCombinedFixedWindowAndTokenRateLimiting()
		{
			// Fixed window rule: allows up to 5 requests in a 1-minute window
			var fixedWindowRule = new FixedWindowRateLimitRule(5, TimeSpan.FromMinutes(1));

			// Token-based rule: allows up to 3 requests for the token in a 1-minute window
			var tokenRule = new TokenRateLimitRule(3, TimeSpan.FromMinutes(1));

			_rateLimiterManager.AddRule("resourceA", fixedWindowRule);
			_rateLimiterManager.AddRule("resourceA", tokenRule);

			var clientId = "client1";
			var resource = "resourceA";
			var region = "US";
			var token = "token123";

			// The first 3 requests should be allowed (because of the TokenRateLimitRule)
			for (int i = 0; i < 3; i++)
			{
				Assert.That(_rateLimiterManager.IsRequestAllowed(clientId, resource, region, token), Is.True);
			}

			// The 4th and 5th requests should still be allowed by the FixedWindowRateLimitRule
			// even though the TokenRateLimitRule would deny further requests
			for (int i = 3; i < 5; i++)
			{
				Assert.That(_rateLimiterManager.IsRequestAllowed(clientId, resource, region, token), Is.True);
			}

			// The 6th request should be denied by the FixedWindowRateLimitRule
			Assert.That(_rateLimiterManager.IsRequestAllowed(clientId, resource, region, token), Is.False);
		}


		[Test]
		public void TestCombinedSlidingWindowAndTokenRateLimiting()
		{
			// Sliding window rule: allows up to 5 requests in a 15-second window
			var slidingWindowRule = new SlidingWindowRateLimitRule(5, TimeSpan.FromSeconds(15));

			// Token-based rule: allows up to 3 requests for the token in a 1-minute window
			var tokenRule = new TokenRateLimitRule(3, TimeSpan.FromMinutes(1));

			_rateLimiterManager.AddRule("resourceB", slidingWindowRule);
			_rateLimiterManager.AddRule("resourceB", tokenRule);

			var clientId = "client1";
			var resource = "resourceB";
			var region = "EU";
			var token = "tokenABC";

			// The first 3 requests should be allowed by both the SlidingWindowRateLimitRule and the TokenRateLimitRule
			for (int i = 0; i < 3; i++)
			{
				Assert.That(_rateLimiterManager.IsRequestAllowed(clientId, resource, region, token), Is.True);
			}

			// The 4th and 5th requests should be allowed by the SlidingWindowRateLimitRule
			// even though the TokenRateLimitRule would deny further requests
			for (int i = 3; i < 5; i++)
			{
				Assert.That(_rateLimiterManager.IsRequestAllowed(clientId, resource, region, token), Is.True);
			}

			// The 6th request should be denied by the SlidingWindowRateLimitRule
			Assert.That(_rateLimiterManager.IsRequestAllowed(clientId, resource, region, token), Is.False);
		}

	}
}
