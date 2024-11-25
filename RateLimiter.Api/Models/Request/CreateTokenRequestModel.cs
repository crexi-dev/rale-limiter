using RateLimiter.Core.Domain.Enums;

namespace RateLimiter.Api.Models.Request
{
	public class CreateTokenRequestModel
	{
		public RegionType Type { get; set; }
	}
}
