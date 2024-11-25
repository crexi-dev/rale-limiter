using RateLimiter.Core.Domain.Enums;
using RateLimiter.Core.Settings;

namespace RateLimiter.BusinessLogic.Services
{
	public interface ITokenService
	{
		IKeyGeneratorService KeyGeneratorService { get; }
		TokenSettings TokenSettings { get; }
		Task<string> GenerateTokenAsync(RegionType type);
	}
}
