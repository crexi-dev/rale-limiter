using System.Collections.Generic;
using System;
using System.Linq;

namespace RateLimiter;

/// <summary>
/// Rule to limit the certain timespan has passed since the last call
/// </summary>
public class TimespanSinceLastCallRule(TimeSpan requiredTimespan) : BaseRule
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

		// If the timespan from the last denied request has not passed yet, allow the request
		if (lastDeniedRequest != null && now - lastDeniedRequest.Timestamp <= requiredTimespan)
			return true;

		var lastRequest = CommonLog?.LastOrDefault(entry => 
			entry.ClientId == clientId 
			&& (Factors == null 
				|| entry.Factors?.ContainsAllElements(Factors) == true));

		return lastRequest == null || now - lastRequest.Timestamp >= requiredTimespan;
	}

}




