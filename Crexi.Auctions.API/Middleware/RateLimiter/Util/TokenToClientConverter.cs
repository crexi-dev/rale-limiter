using Crexi.API.Common.RateLimiter.Models;
using System.IdentityModel.Tokens.Jwt;

public class TokenToClientConverter : ITokenToClientConverter
{
    public Client ConvertTokenToClient(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Extract client ID and location from token claims
            var clientId = jwtToken.Claims.FirstOrDefault(c => c.Type == "client_id")?.Value;
            var clientLocation = jwtToken.Claims.FirstOrDefault(c => c.Type == "location")?.Value;

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientLocation))
            {
                return null;
            }

            return new Client(clientId, clientLocation);
        }
        catch (Exception ex)
        {
            // TODO logging
            return null;
        }
    }
}
