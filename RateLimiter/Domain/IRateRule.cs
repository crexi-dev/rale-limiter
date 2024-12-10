using System.Threading.Tasks;

namespace RateLimiter.Domain;

public interface IRateRule
{ 
    Task<CheckStatus> Check(string token, ApiEndpoint endpoint);
}