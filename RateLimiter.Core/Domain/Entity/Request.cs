using RateLimiter.Core.Domain.Enums;

namespace RateLimiter.Core.Domain.Entity
{
	public class Request
	{
		public Guid Id { get; set; }
		public RegionType RegionType { get; set; }
		public string UrlPath { get; set; }
		public DateTime DateTime { get; set; }
	}
}
