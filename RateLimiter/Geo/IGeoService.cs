using System.Threading.Tasks;

namespace RateLimiter.Geo;
public interface IGeoService
{
    Task<string> GetLocationAsync(string clientId);
}
