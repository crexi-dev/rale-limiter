using Crexi.API.Common.RateLimiter.Models;

public interface ITokenToClientConverter
{
    Client ConvertTokenToClient(string token);
}