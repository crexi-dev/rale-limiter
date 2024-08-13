using System;

namespace Crexi.Common.RateLimiter.Rules
{
	/// <summary>
	/// This rule allows X number of requests over a specific time span. For example: Allow 1000 requests per day
	/// </summary>
	public class FixedWindowRule : IRateLimitRule
	{
		private readonly int MaxRequests;
		private readonly TimeSpan WindowSize;
		private int RequestCount;
		private DateTime WindowStart;

		public FixedWindowRule(int maxRequests, TimeSpan windowSize)
		{
			MaxRequests = maxRequests;
			WindowSize = windowSize;
			WindowStart = DateTime.UtcNow;
			RequestCount = 0;
		}

		public bool Allowed()
		{
			var now = DateTime.UtcNow;

			if (now - WindowStart > WindowSize)
			{
				// Reset the window
				WindowStart = now;
				RequestCount = 1;
				return true;
			}

			if (RequestCount < MaxRequests)
			{
				RequestCount++;
				return true;
			}

			return false;
		}
	}
}
