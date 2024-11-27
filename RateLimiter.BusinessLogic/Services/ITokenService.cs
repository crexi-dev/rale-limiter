using RateLimiter.Core.Domain.Enums;

namespace RateLimiter.BusinessLogic.Services
{
	public interface ITokenService
	{
		Task<string> GenerateTokenAsync(RegionType type);
	}
}
