
namespace RateLimiter.Interfaces
{
    public interface IRateLimitRule
    {
        bool IsRequestAllowed(string clientId, string resource);
        bool IsRequestAllowed(string clientId, string resource, string ip);
    }
}
