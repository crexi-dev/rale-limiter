using System;
using System.Collections.Generic;
using System.Linq;

namespace RateLimiter;

/// <summary>
/// This rule limits the number of requests a client can make within a specified timespan.
/// It checks the number of requests made by a client within the given timespan and denies further requests if the limit is exceeded.
/// </summary>
public class XRequestsPerTimespanRule(int maxRequests, TimeSpan timespan) : BaseRule
{
	public override bool IsRequestAllowed(string clientId, Dictionary<string, string>? factors)
	{
		if (base.IsRequestAllowed(clientId, factors))
			return true;

		var now = DateTime.UtcNow;
		var lastDeniedRequest = CommonLog?.LastOrDefault(entry =>
			entry.ClientId == clientId
			&& entry.IsAllowed == false
			&& (Factors == null
			    || entry.Factors?.ContainsAllElements(Factors) == true));

		//first time of the log entry that should pass the timespan 
		var lastTime = now - timespan;
		//chose the latest denied time as the start time for the count if it passes the timespan
		if (lastDeniedRequest != null && lastDeniedRequest.Timestamp > lastTime)
			lastTime = lastDeniedRequest.Timestamp;

		var requestsCount = CommonLog?.Count(entry => entry.ClientId == clientId && entry.Timestamp > lastTime);

		return requestsCount < maxRequests;
	}

	
}


