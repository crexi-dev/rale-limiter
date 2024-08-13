namespace Crexi.Common.RateLimiter.Rules
{
	/// <summary>
	/// This rule allows all requests
	/// </summary>
	public class BypassRule : IRateLimitRule
	{
		public bool Allowed()
		{
			return true;
		}
	}
}
