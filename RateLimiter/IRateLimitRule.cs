using System;

namespace Crexi.Common.RateLimiter
{
	public interface IRateLimitRule
	{
		bool Allowed();

		public enum RuleTypes
		{
			FixedWindow,
			SlidingWindow,
			Bypass
		}

		public enum RuleScope
		{
			Client,
			Session,
			GeoRegion, //For future use: To throttle or rate limit based on geographical location
			Endpoint //For future use: To rate limit based on a specific endpoint or a resource regardless of client or user
		}

		public struct RuleDefinition //More sophisticated definition structure is needed to allow for leakybucket or token bucket rules
		{
			public RuleTypes Type { get; set; }
			public RuleScope Scope { get; set; }
			public TimeSpan WindowSize { get; set; }
			public int MaxAllowed { get; set; }
			public DateTime StartTime { get; set; } // Optional: For scheduling rules
			public DateTime EndTime { get; set; } // Optional: For scheduling rules
		}
	}
}
