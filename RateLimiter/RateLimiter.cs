using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace RateLimiter;


/// <summary>
/// Class to limit the number of requests
/// </summary>
public class RateLimiter
{
	private readonly Dictionary<string, List<IRateLimitingRule>> _resourceRules = new();
	private readonly List<IRateLimitingRule> _globalRules = [];
	private readonly ConcurrentQueue<RequestLogEntry> _requestLog = []; //todo: use cache

	/// <summary>
	/// Time to keep the log entries
	/// </summary>
	public TimeSpan LogRetentionTime { get; set; } = TimeSpan.FromDays(1); //todo: make it configurable


	/// <summary>
	/// Add a rule for a specific resource
	/// </summary>
	/// <param name="resource"></param>
	/// <param name="rule"></param>
	public void AddRule(string resource, IRateLimitingRule rule)
	{
		if (!_resourceRules.ContainsKey(resource)) 
			_resourceRules[resource] = [];

		rule.CommonLog = _requestLog;
		_resourceRules[resource].Add(rule);
	}


	/// <summary>
	/// Add a global rule
	/// </summary>
	/// <param name="rule"></param>
	public void AddGlobalRule(IRateLimitingRule rule)
	{
		_globalRules.Add(rule);
	}


	/// <summary>
	/// Remove old entries from the log
	/// </summary>
	private void RemoveOldEntries()
	{
		var now = DateTime.UtcNow;
		while (_requestLog.TryPeek(out var first))
		{
			if (now - first.Timestamp <= LogRetentionTime)
				break;
			_requestLog.TryDequeue(out _);
		}
	}


	/// <summary>
	/// Check if a request is allowed
	/// </summary>
	/// <param name="resource"></param>
	/// <param name="clientId"></param>
	/// <param name="factors"></param>
	/// <returns></returns>
	public bool IsRequestAllowed(string resource, string clientId, Dictionary<string, string>? factors)
	{
		var rulesToCheck = new List<IRateLimitingRule>(_globalRules);

		if (_resourceRules.TryGetValue(resource, out var resourceRule)) 
			rulesToCheck.AddRange(resourceRule);

		RemoveOldEntries();

		var isAllowed = rulesToCheck.All(rule => rule.IsRequestAllowed(clientId, factors));

		var entry = new RequestLogEntry
		{
			ClientId = clientId,
			Resource = resource,
			Timestamp = DateTime.UtcNow,
			IsAllowed = isAllowed,	
		};
		if (factors != null) 
			entry.Factors = new Dictionary<string, string>(factors);
		_requestLog.Enqueue(entry);

		return isAllowed;
	}


	/// <summary>
	/// Get the request log
	/// </summary>
	/// <returns></returns>
	public IEnumerable<RequestLogEntry>? GetRequestLog()
	{
		return _requestLog?.ToArray();
	}


}




