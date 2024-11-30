using Newtonsoft.Json;

namespace RateLimiter.Api.Infrastructure.ExeptionHandling.Model
{
	public class ErrorModel
	{
		public int Status { get; set; }
		public required string Title { get; set; }
		public required string Details { get; set; }
		public override string ToString() => JsonConvert.SerializeObject(this);
	}
}
