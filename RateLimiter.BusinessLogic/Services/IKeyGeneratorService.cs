using Microsoft.IdentityModel.Tokens;

namespace RateLimiter.BusinessLogic.Services
{
	public interface IKeyGeneratorService
	{
		string SigningAlgorithm { get; }
		SecurityKey GetKey();
	}
}
