namespace RateLimiter.Core.Settings
{
	public class TokenSettings
	{
		public required string Issuer { get; set; }
		public required string Audience { get; set; }
	}
}
