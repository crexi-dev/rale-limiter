using Crexi.Common.RateLimiter;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("RateLimiter.Tests")]
namespace Crexi.Common
{
	public static class SessionManager
	{
		private static Dictionary<string, UserContext> sessions = new Dictionary<string, UserContext>();
		private static Dictionary<string, string> users = new Dictionary<string, string>();
		internal static TimeSpan SessionTimeout = new TimeSpan(0, 20, 0);

		/// <summary>
		/// This method authenticates the user given a username, password, and clientid
		/// </summary>
		/// <param name="username">Username to authenticate</param>
		/// <param name="password">Password to authenticate</param>
		/// <param name="clientId">ClientId to authenticate</param>
		/// <returns>Returns a token string to be used by caller for subsequent calls</returns>
		/// <exception cref="ApplicationException">All other errors</exception>
		public static string Authenticate(string username, string password, string clientId)
		{
			try
			{
				string userKey = GetUserKey(username, clientId);
				if (users.ContainsKey(userKey))
				{
					var token = users[userKey];
					if (sessions.ContainsKey(token) && sessions[token].TokenExpiry > DateTime.Now)
					{ //User already authenticated
						return token;
					}
					else //clean up dictioanries and authenticate
					{
						sessions.Remove(token);
						users.Remove(userKey);
					}
				}

				if (AuthenticateCore(username, password, clientId)) //Authenticate user and return token
				{
					UserContext u = new UserContext();
					u.UserName = username;
					u.ClientId = clientId;
					u.UserKey = userKey;
					u.Token = Guid.NewGuid().ToString();
					u.TokenExpiry = DateTime.Now.AddSeconds(SessionTimeout.TotalSeconds);

					if (users.ContainsKey(userKey)) //Some maintenance cleanup just in case
						LogOffUser(username, clientId);

					users.Add(userKey, u.Token);
					sessions.Add(u.Token, u);

					RateLimiterBase.RegisterSession(u); //Register this Session with RateLimiter

					return u.Token;
				}
				else //Throw authentication error
				{
					var ex = new Exception("Authentication Error");
					ex.Data.Add("message", "Bad username/client or password");
					ex.Data.Add("username", username);
					ex.Data.Add("clientId", clientId);
					throw ex;
				}
			}
			catch (Exception ex)
			{
				// Todo: Log exception
				throw new ApplicationException("An error occurred during authentication.", ex);
			}
		}

		public static string GetUserKey(string username, string clientId)
		{
			return (clientId + "|" + username).ToLower();
		}

		private static bool AuthenticateCore(string username, string password, string clientId) //In reality this should look up the user/password in a db or similar
		{
			return (username.ToLower() == "testuser" && password == "secretpassword" && clientId.ToLower() == "our#1client");
		}

		/// <summary>
		/// This method validates a token and checks applicable rate limiting rules
		/// </summary>
		/// <param name="token">Token to be validated</param>
		/// <returns>True if token is valid and false if token is not valid</returns>
		/// <exception cref="CrexiRateLimitExceededException">Throws CrexiRateLimitExceededException if rate limit is exceeded ot not allowed</exception>
		/// <exception cref="ApplicationException">All other errors</exception>
		public static bool ValidateToken(string token)
		{
			try
			{
				if (sessions.ContainsKey(token))
				{
					var user = sessions[token];
					if (user.TokenExpiry > DateTime.Now)
					{
						// Rate limit check here
						if (RateLimiterBase.Allowed(user))
							return true;
						else
						{
							var ex = new CrexiRateLimitExceededException("User not allowed. Rate limit exceeded.");
							ex.Data.Add("token", token);
							throw ex;
						}
					}
					else
					{
						// Clean up dictionaries of expired tokens and sessions
						sessions.Remove(token);
						users.Remove(user.UserKey);
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				// Todo: Log exception
				throw new ApplicationException("An error occurred during token validation.", ex);
			}
		}

		/// <summary>
		/// This method invalidates an existing user token
		/// </summary>
		/// <param name="token">Token to be invalidated</param>
		/// <exception cref="ApplicationException">All other errors</exception>
		public static void InvalidateToken(string token)
		{
			try
			{
				if (sessions.ContainsKey(token))
				{
					var user = sessions[token];
					var userKey = user.UserKey;
					users.Remove(userKey);
					sessions.Remove(token);
				}
			}
			catch (Exception ex)
			{
				//Todo: Log exception
				throw new ApplicationException("An error occurred while invalidating the token.", ex);
			}
		}

		/// <summary>
		/// This method loggs off the user and invalidates the token
		/// </summary>
		/// <param name="username">The user name to logg off</param>
		/// <param name="clientId">The user's client id</param>
		/// <exception cref="ApplicationException">All other errors</exception>
		public static void LogOffUser(string username, string clientId)
		{
			try
			{
				string userKey = GetUserKey(username, clientId);
				if (users.ContainsKey(userKey))
				{
					var token = users[userKey];
					InvalidateToken(token);
				}
			}
			catch (Exception ex)
			{
				//Todo: Log exception
				throw new ApplicationException("An error occurred while logging off the user.", ex);
			}
		}
	}
}