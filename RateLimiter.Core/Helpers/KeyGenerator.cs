using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace RateLimiter.Core.Helpers
{
	public class KeyGenerator
	{
		private readonly SymmetricSecurityKey _secretKey;
		public string SigningAlgorithm => SecurityAlgorithms.HmacSha256;

		public KeyGenerator(string key)
		{
			var secretBytes = Encoding.UTF8.GetBytes(key);
			_secretKey = new SymmetricSecurityKey(secretBytes);
		}

		public SecurityKey GetKey() => _secretKey;
	}
}
