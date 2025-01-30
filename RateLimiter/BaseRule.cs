using System.Collections.Generic;
using System.Linq;

namespace RateLimiter;


/// <summary>
///	Interface for rate limiting rules
/// </summary>
public interface IRateLimitingRule
{
	bool IsRequestAllowed(string clientId, Dictionary<string, string>? factors = null);
	Dictionary<string, string>? Factors { get; set; }
	IEnumerable<RequestLogEntry> CommonLog { get; set; }
}


/// <summary>
/// Base class for rate limiting rules
/// </summary>
public abstract class BaseRule : IRateLimitingRule
{
	/// <summary>
	/// Factors that can be used to determine if the rule is applicable
	/// </summary>
	public Dictionary<string, string>? Factors { get; set; }
	
	
	/// <summary>
	/// Common log of requests
	/// </summary>
	public IEnumerable<RequestLogEntry> CommonLog { get; set; }

	/// <summary>
	/// Check if the request is allowed
	/// </summary>
	/// <param name="clientId"></param>
	/// <param name="factors"></param>
	/// <returns></returns>
	public virtual bool IsRequestAllowed(string clientId, Dictionary<string, string>? factors)
	{
		// If factors are not set or are not used, the rule is not applicable
		if (Factors != null && factors?.ContainsAllElements(Factors) != true)
			return true;
		return false;
	}
}


/// <summary>
/// Extension methods for dictionaries
/// </summary>
public static class DictionaryComparer
{
	public static bool ContainsAllElements<TKey, TValue>(
		this Dictionary<TKey, TValue> mainDict,
		Dictionary<TKey, TValue> subDict)
	{
		return subDict.All(kv => mainDict.ContainsKey(kv.Key) && EqualityComparer<TValue>.Default.Equals(mainDict[kv.Key], kv.Value));
	}
}