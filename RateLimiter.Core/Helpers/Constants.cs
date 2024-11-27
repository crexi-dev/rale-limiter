using RateLimiter.Core.Domain.Enums;

namespace RateLimiter.Core.Helpers
{
	public static class Constants
	{
		public const string SecretKey = "GeneratorSecretKey";

		public static class CustomClaimTypes
		{
			public const string UniqueIdentifier = "IdentifierId";
			public const string Region = nameof(RegionType);
		}
	}
}
