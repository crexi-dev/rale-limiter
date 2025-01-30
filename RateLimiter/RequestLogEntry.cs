using System;
using System.Collections.Generic;

namespace RateLimiter
{
	/// <summary>
	/// Class to limit the number of requests per timespan
	/// </summary>
	public class RequestLogEntry
	{
		public string ClientId { get; set; }
		public string Resource { get; set; }
		public DateTime Timestamp { get; set; }
		public bool IsAllowed { get; set; }
		public Dictionary<string, string>? Factors { get; set; }
	}
}
