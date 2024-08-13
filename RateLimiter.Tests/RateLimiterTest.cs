using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using static Crexi.Common.RateLimiter.IRateLimitRule;

namespace Crexi.Common.RateLimiter;

[TestFixture]
public class RateLimiterTest
{
	[Test]
	public void SessionManager_AuthenticateSuccess()
	{
		string token = "";
		Assert.DoesNotThrow(() =>
		{
			token = SessionManager.Authenticate("testuser", "secretpassword", "our#1client");
		});
		Assert.IsNotEmpty(token);
		SessionManager.InvalidateToken(token);
	}

	[Test]
	public void SessionManager_AuthenticateFailure()
	{
		string token = "";
		Assert.Throws<System.ApplicationException>(() =>
		{
			token = SessionManager.Authenticate("", "", "");
		});
		Assert.IsEmpty(token);
	}

	[Test]
	public void SessionManager_ValidateToken_Success()
	{
		string token = "";
	
		token = SessionManager.Authenticate("testuser", "secretpassword", "our#1client");
		Assert.IsNotEmpty(token);
		Assert.IsTrue(SessionManager.ValidateToken(token));
		SessionManager.InvalidateToken(token);
	}

	[Test]
	public void SessionManager_ValidateToken_Failure()
	{
		string token = "";
		Assert.IsFalse(SessionManager.ValidateToken(token));
	}

	[Test]
	public void SessionManager_ValidateToken_Expired()
	{
		string token = "";

		SessionManager.SessionTimeout = new System.TimeSpan(0, 0, 1);
		token = SessionManager.Authenticate("testuser", "secretpassword", "our#1client");

		Assert.IsNotEmpty(token);
		Assert.IsTrue(SessionManager.ValidateToken(token));
		Thread.Sleep(1500);
		Assert.IsFalse(SessionManager.ValidateToken(token));
		SessionManager.InvalidateToken(token);
		SessionManager.SessionTimeout = new System.TimeSpan(0, 20, 0);
	}

	[Test]
	public void RateLimiterBase_Allowed_ClientRule_Allowed()
	{
		// Arrange
		var user = new UserContext
		{
			UserName = "testuser",
			ClientId = "client1",
			Token = "token1",
			TokenExpiry = DateTime.Now.AddMinutes(20)
		};
		RateLimiterBase.RegisterSession(user);

		// Simulate a client-level rule that always allows requests and a session rule that denies it
		var ruleDef = new RuleDefinition { Type = RuleTypes.Bypass, Scope = RuleScope.Client };
		var rule = RateLimiterBase.CreateRule(ruleDef);
		RateLimiterBase.ClientRules[user.ClientId] = new List<IRateLimitRule> { rule };
		ruleDef = new RuleDefinition { Type = RuleTypes.FixedWindow, MaxAllowed = 0, WindowSize = new TimeSpan(1, 0, 0), Scope = RuleScope.Session };
		rule = RateLimiterBase.CreateRule(ruleDef);
		RateLimiterBase.SessionRules[user.Token] = new List<IRateLimitRule> { rule };
		// Act
		var result = RateLimiterBase.Allowed(user);

		// Assert
		Assert.IsTrue(result, "The client rule should allow the request.");
	}

	[Test]
	public void RateLimiterBase_Allowed_SessionRule_Allowed()
	{
		// Arrange
		var user = new UserContext
		{
			UserName = "testuser",
			ClientId = "client1",
			Token = "token2",
			TokenExpiry = DateTime.Now.AddMinutes(20)
		};
		RateLimiterBase.RegisterSession(user);

		// Simulate a session-level rule that always allows requests and another session rule that denies it
		var ruleDef = new RuleDefinition { Type = RuleTypes.Bypass, Scope = RuleScope.Session };
		var rule = RateLimiterBase.CreateRule(ruleDef);
		var rules = new List<IRateLimitRule> { rule };
		ruleDef = new RuleDefinition { Type = RuleTypes.FixedWindow, MaxAllowed = 0, WindowSize = new TimeSpan(1, 0, 0), Scope = RuleScope.Session };
		rule = RateLimiterBase.CreateRule(ruleDef);
		rules.Add(rule);
		RateLimiterBase.SessionRules[user.Token] = rules;

		// Act
		var result = RateLimiterBase.Allowed(user);

		// Assert
		Assert.IsTrue(result, "The session rule should allow the request.");
	}

	[Test]
	public void RateLimiterBase_Allowed_NotAllowed()
	{
		// Arrange
		var user = new UserContext
		{
			UserName = "testuser",
			ClientId = "client2",
			Token = "token3",
			TokenExpiry = DateTime.Now.AddMinutes(20)
		};
		RateLimiterBase.RegisterSession(user);

		// Simulate a client-level rule that denies requests and a session rule that allows it
		var ruleDef = new RuleDefinition { Type = RuleTypes.FixedWindow, MaxAllowed = 0, WindowSize = new TimeSpan(1, 0, 0), Scope = RuleScope.Client };
		var rule = RateLimiterBase.CreateRule(ruleDef);
		RateLimiterBase.ClientRules[user.ClientId] = new List<IRateLimitRule> { rule };
		ruleDef = new RuleDefinition { Type = RuleTypes.FixedWindow, MaxAllowed = 100, WindowSize = new TimeSpan(1, 0, 0), Scope = RuleScope.Session };
		rule = RateLimiterBase.CreateRule(ruleDef);
		RateLimiterBase.SessionRules[user.Token] = new List<IRateLimitRule> { rule };

		// Act
		var result = RateLimiterBase.Allowed(user);

		// Assert
		Assert.IsFalse(result, "No rules should allow the request, so it should be disallowed.");
	}

	[Test]
	public void RateLimiterBase_RegisterSession_ShouldAddClientAndSessionRules()
	{
		// Arrange
		var user = new UserContext
		{
			UserName = "testuser",
			ClientId = "client3",
			Token = "token4",
			TokenExpiry = DateTime.Now.AddMinutes(20)
		};

		//Clear Rules Dictionaries
		RateLimiterBase.ClientRules.Clear();
		RateLimiterBase.SessionRules.Clear();

		// Act
		RateLimiterBase.RegisterSession(user);

		// Assert
		Assert.IsTrue(RateLimiterBase.ClientRules.ContainsKey(user.ClientId), "Client rules should be registered.");
		Assert.IsTrue(RateLimiterBase.SessionRules.ContainsKey(user.Token), "Session rules should be registered.");
	}

	[Test]
	public void RateLimiterBase_UnRegisterSession_ShouldRemoveSessionRulesButNotClientRules()
	{
		// Arrange
		var user = new UserContext
		{
			UserName = "testuser",
			ClientId = "client4",
			Token = "token5",
			TokenExpiry = DateTime.Now.AddMinutes(20)
		};

		//Clear Rules Dictionaries
		RateLimiterBase.ClientRules.Clear();
		RateLimiterBase.SessionRules.Clear();

		// Act
		RateLimiterBase.RegisterSession(user);
		RateLimiterBase.UnRegisterSession(user);

		// Assert
		Assert.IsFalse(RateLimiterBase.SessionRules.ContainsKey(user.Token), "Session rules should be unregistered.");
		Assert.IsTrue(RateLimiterBase.ClientRules.ContainsKey(user.ClientId), "Client rules should not be unregistered.");
	}

	[Test]
	public void FixedWindowRule_Allowed()
	{
		// Arrange
		var user = new UserContext
		{
			UserName = "testuser",
			ClientId = "client2",
			Token = "token3",
			TokenExpiry = DateTime.Now.AddMinutes(20)
		};
		RateLimiterBase.RegisterSession(user);

		//Clear Rules Dictionaries
		RateLimiterBase.ClientRules.Clear();
		RateLimiterBase.SessionRules.Clear();

		// Simulate a Session-level rule that allows 2 requests per minute. Test will try 3 and should get 2 allowed and 1 denied
		var ruleDef = new RuleDefinition { Type = RuleTypes.FixedWindow, MaxAllowed = 100, WindowSize = new TimeSpan(0, 1, 0), Scope = RuleScope.Client };
		var rule = RateLimiterBase.CreateRule(ruleDef);
		RateLimiterBase.ClientRules[user.ClientId] = new List<IRateLimitRule> { rule };
		ruleDef = new RuleDefinition { Type = RuleTypes.FixedWindow, MaxAllowed = 2, WindowSize = new TimeSpan(0, 0, 1), Scope = RuleScope.Session };
		rule = RateLimiterBase.CreateRule(ruleDef);
		RateLimiterBase.SessionRules[user.Token] = new List<IRateLimitRule> { rule };

		// Assert
		Assert.IsTrue(RateLimiterBase.Allowed(user), "Request should be allowed.");
		Assert.IsTrue(RateLimiterBase.Allowed(user), "Request should be allowed.");
		Assert.IsFalse(RateLimiterBase.Allowed(user), "Rule should disallow reqest after 2 attempts.");
		Thread.Sleep(1000);
		Assert.IsTrue(RateLimiterBase.Allowed(user), "Request should be allowed.");
		Assert.IsTrue(RateLimiterBase.Allowed(user), "Request should be allowed.");
		Assert.IsFalse(RateLimiterBase.Allowed(user), "Rule should disallow reqest after 2 attempts.");
	}

	[Test]
	public void SlidingWindowRule_Allowed()
	{
		// Arrange
		var user = new UserContext
		{
			UserName = "testuser",
			ClientId = "client2",
			Token = "token3",
			TokenExpiry = DateTime.Now.AddMinutes(20)
		};
		RateLimiterBase.RegisterSession(user);

		//Clear Rules Dictionaries
		RateLimiterBase.ClientRules.Clear();
		RateLimiterBase.SessionRules.Clear();

		// Simulate a Session-level rule that allows 2 requests per minute. Test will try 3 and should get 2 allowed and 1 denied
		var ruleDef = new RuleDefinition { Type = RuleTypes.FixedWindow, MaxAllowed = 100, WindowSize = new TimeSpan(0, 1, 0), Scope = RuleScope.Client };
		var rule = RateLimiterBase.CreateRule(ruleDef);
		RateLimiterBase.ClientRules[user.ClientId] = new List<IRateLimitRule> { rule };
		ruleDef = new RuleDefinition { Type = RuleTypes.SlidingWindow, MaxAllowed = 2, WindowSize = new TimeSpan(0, 0, 1), Scope = RuleScope.Session };
		rule = RateLimiterBase.CreateRule(ruleDef);
		RateLimiterBase.SessionRules[user.Token] = new List<IRateLimitRule> { rule };

		// Assert
		Assert.IsTrue(RateLimiterBase.Allowed(user), "Request should be allowed.");
		Assert.IsTrue(RateLimiterBase.Allowed(user), "Request should be allowed.");
		Assert.IsFalse(RateLimiterBase.Allowed(user), "Rule should disallow reqest after 2 attempts.");
		Thread.Sleep(1000);
		Assert.IsTrue(RateLimiterBase.Allowed(user), "Request should be allowed.");
		Assert.IsTrue(RateLimiterBase.Allowed(user), "Request should be allowed.");
		Assert.IsFalse(RateLimiterBase.Allowed(user), "Rule should disallow reqest after 2 attempts.");
	}
}