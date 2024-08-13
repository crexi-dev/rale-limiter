using System;
using System.Collections.Generic;

namespace Crexi.Common.RateLimiter.Rules
{
	/// <summary>
	/// This rule allows X number of requests over a rolling time span. For example: allow no more than 100 requests in any 1 minute window.
	/// </summary>
	public class SlidingWindowRule : IRateLimitRule
	{
		private readonly int MaxRequests;
		private readonly TimeSpan WindowSize;
		private readonly Queue<DateTime> RequestTimes;

		public SlidingWindowRule(int maxRequests, TimeSpan windowSize)
		{
			MaxRequests = maxRequests;
			WindowSize = windowSize;
			RequestTimes = new Queue<DateTime>();
		}

		public bool Allowed()
		{
			var now = DateTime.UtcNow;

			while (RequestTimes.Count > 0 && now - RequestTimes.Peek() > WindowSize)
				RequestTimes.Dequeue();

			if (RequestTimes.Count < MaxRequests)
			{
				RequestTimes.Enqueue(now);
				return true;
			}

			return false;
		}
	}
}
