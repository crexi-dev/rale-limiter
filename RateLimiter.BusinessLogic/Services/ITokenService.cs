using RateLimiter.Core.Domain.Enums;
using RateLimiter.Core.Helpers;
using RateLimiter.Core.Settings;

namespace RateLimiter.BusinessLogic.Services
{
	public interface ITokenService
	{
		KeyGenerator KeyGenerator { get; }
		TokenSettings TokenSettings { get; }
		Task<string> GenerateTokenAsync(RegionTokenType type);
	}
}
