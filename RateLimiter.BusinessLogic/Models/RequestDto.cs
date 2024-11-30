using RateLimiter.Core.Domain.Enums;

namespace RateLimiter.BusinessLogic.Models
{
	public class RequestDto
	{
		public required Guid Id { get; init; }
		public required RegionType RegionType { get; init; }
		public required string UrlPath { get; init; }
		public required DateTime DateTime { get; init; }
	}
}
