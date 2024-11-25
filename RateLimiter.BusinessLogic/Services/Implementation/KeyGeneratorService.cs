using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace RateLimiter.BusinessLogic.Services.Implementation
{
	public class KeyGeneratorService : IKeyGeneratorService
	{
		private readonly SymmetricSecurityKey _secretKey;
		public string SigningAlgorithm => SecurityAlgorithms.HmacSha256;

		public KeyGeneratorService(string key)
		{
			var secretBytes = Encoding.UTF8.GetBytes(key);
			_secretKey = new SymmetricSecurityKey(secretBytes);
		}

		public SecurityKey GetKey() => _secretKey;
	}
}
