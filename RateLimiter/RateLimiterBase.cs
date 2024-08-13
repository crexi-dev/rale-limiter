using Crexi.Common.RateLimiter.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using static Crexi.Common.RateLimiter.IRateLimitRule;

[assembly: InternalsVisibleTo("RateLimiter.Tests")]
namespace Crexi.Common.RateLimiter
{
	public class RateLimiterBase
	{
		/// <summary>
		/// Dictionary to store session-based rules (keyed by token)
		/// </summary>
		internal static readonly Dictionary<string, List<IRateLimitRule>> SessionRules = new Dictionary<string, List<IRateLimitRule>>();

		/// <summary>
		/// Dictionary to store client-based rules (keyed by clientId) 
		/// </summary>
		internal static readonly Dictionary<string, List<IRateLimitRule>> ClientRules = new Dictionary<string, List<IRateLimitRule>>();

		/// <summary>
		/// Method to check if the request is allowed based on the user object.
		/// </summary>
		/// <param name="user">The user object for the user who is trying to execute a request</param>
		/// <returns>This method will execute all the client rules, if any of them pass then it will execute all the session based rules, if any of them pass it will return true.
		/// If all client rules did not pass, this method will return false without checking the session rules. If any of the client rules pass and none of the session rules pass it will return false.</returns>
		/// <exception cref="ApplicationException">All other errors</exception>
		public static bool Allowed(UserContext user)
		{
			try
			{
				var isAllowed = false;

				// Check client rules based on the clientId
				if (ClientRules.TryGetValue(user.ClientId, out var clientRules))
				{
					foreach (var rule in clientRules)
					{
						if (rule is BypassRule) return true; //shortcutting the BypassRule on Client level
						if (rule.Allowed())
							isAllowed = true;
					}
				}
				if (!isAllowed) return false;//If request is not allowed on the client level we skip session check and return false.
				isAllowed = false;
				// Check session rules based on the token
				if (SessionRules.TryGetValue(user.Token, out var sessionRules))
				{
					foreach (var rule in sessionRules)
					{
						if (rule is BypassRule) return true; //shortcutting the BypassRule on Session level
						if (rule.Allowed())
							isAllowed = true;
					}
				}
				return isAllowed;
			}
			catch (Exception ex)
			{
				// Todo: Log exception
				throw new ApplicationException("An error occurred while evaluating rate limit rules.", ex);
			}
		}

		/// <summary>
		/// Method to register a new session and create the necessary rules. This method will look up applicable rules per user, create the rule objects with the right properties for both the session and client. 
		/// Client rules may already exist. Then it will add the rules into the dictionaries for lookup. We can add additional parameters to rate limit based on endpoint for example
		/// </summary>
		/// <param name="user">The UserContext object here allows us to ratelimit based on user and related client properties. Context can have requested endpoint or geo location, etc.</param>
		/// <exception cref="ApplicationException">All other errors</exception>
		public static void RegisterSession(UserContext user)
		{
			try
			{
				// Example: Lookup or create rules based on user properties
				List<RuleDefinition> ruleDefinitions = GetRuleDefinitions(user);

				RegisterSessionByUserRules(user, ruleDefinitions);
			}
			catch (Exception ex)
			{
				// Todo: Log exception
				throw new ApplicationException("An error occurred while registering session.", ex);
			}
		}

		internal static void RegisterSessionByUserRules(UserContext user, List<RuleDefinition> ruleDefinitions)
		{
			try
			{
				// Add client rule if not already present. Update it if already present
				// Note that if the select below returns an empty list we will still add it to dictionary to update it.
				if (!ClientRules.ContainsKey(user.ClientId))
					ClientRules.Add(user.ClientId, new List<IRateLimitRule>());
				ClientRules[user.ClientId] = ruleDefinitions.Where(r => r.Scope == RuleScope.Client).Select(r => CreateRule(r)).ToList();

				// Add session rule if not already present
				if (!SessionRules.ContainsKey(user.Token))
					SessionRules.Add(user.Token, new List<IRateLimitRule>());
				SessionRules[user.Token] = ruleDefinitions.Where(r => r.Scope == RuleScope.Session).Select(r => CreateRule(r)).ToList();
			}
			catch (Exception ex)
			{
				// Todo: Log exception
				throw new ApplicationException("An error occurred while registering session.", ex);
			}
		}

		/// <summary>
		/// Looks up the applicable rule difinitions based on the UserContext object that's passed in. Ideally this should look up in an In-Memory Database or a Distributed cache like memcached or Redis
		/// </summary>
		/// <param name="user">The user context object to look up rule definitions by</param>
		/// <returns>A list of applicable RuleDefinition objects</returns>
		private static List<RuleDefinition> GetRuleDefinitions(UserContext user)
		{
			//Todo: This is a stub. Implementation is out of scope.
			var ruleDefs = new List<RuleDefinition>();

			var ruleDef = new RuleDefinition();
			ruleDef.MaxAllowed = 100;
			ruleDef.WindowSize = new TimeSpan(0, 1, 0);
			ruleDef.Scope = RuleScope.Session;
			ruleDefs.Add(ruleDef);

			ruleDef = new RuleDefinition();
			ruleDef.MaxAllowed = 100;
			ruleDef.WindowSize = new TimeSpan(24, 0, 0);
			ruleDef.Scope = RuleScope.Client;
			ruleDefs.Add(ruleDef);

			return ruleDefs;
		}

		/// <summary>
		/// This method will remove the session specific rules and will not remove client based rules because there might be other sessions from that same client
		/// </summary>
		/// <param name="user">UserContext needed to unregister their token</param>
		/// <exception cref="ApplicationException">All other errors</exception>
		public static void UnRegisterSession(UserContext user)
		{
			try
			{
				// Remove session-specific rules
				if (SessionRules.ContainsKey(user.Token))
					SessionRules.Remove(user.Token);
			}
			catch (Exception ex)
			{
				// Todo: Log exception
				throw new ApplicationException("An error occurred while unregistering session.", ex);
			}
		}

		/// <summary>
		/// IRateLimitRule Factory. Creates the appropriate iRateLimitRule based on RuleDefinition
		/// </summary>
		/// <param name="ruleDefinition">A RuleDefinition object that defines the rate limiting rule requested and its parameters</param>
		/// <returns>A rate limit object IRateLimitRule</returns>
		/// <exception cref="ArgumentException"></exception>
		public static IRateLimitRule CreateRule(RuleDefinition ruleDefinition)
		{
			try
			{
				switch (ruleDefinition.Type)
				{
					case RuleTypes.FixedWindow:
						return new FixedWindowRule(ruleDefinition.MaxAllowed, ruleDefinition.WindowSize);
					case RuleTypes.SlidingWindow:
						return new SlidingWindowRule(ruleDefinition.MaxAllowed, ruleDefinition.WindowSize);
					case RuleTypes.Bypass:
						return new BypassRule();
					default:
						throw new ArgumentException("Invalid RuleType", nameof(ruleDefinition.Type));
				}
			}
			catch (Exception ex)
			{
				// Todo: Log exception
				throw new ApplicationException("An error occurred while creating a rate limit rule.", ex);
			}
		}
	}

	public class CrexiRateLimitExceededException : Exception
	{
		public CrexiRateLimitExceededException()
		{
		}

		public CrexiRateLimitExceededException(string message)
				: base(message)
		{
		}

		public CrexiRateLimitExceededException(string message, Exception inner)
				: base(message, inner)
		{
		}
	}
}
