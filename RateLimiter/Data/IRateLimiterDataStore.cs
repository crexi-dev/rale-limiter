using RateLimiter.Model;

namespace RateLimiter.Data
{
    public interface IRateLimiterDataStore
    {
        ClientRateLimiterData GetClientData(string clientId, string resource);
        void IncrementRequestCount(string clientId, string resource);
        void ResetClientData(string clientId, string resource);
    }
}
