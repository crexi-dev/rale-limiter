using RateLimiter.Model;

namespace RateLimiter.Interface
{
    public interface IRateLimiterRepository
    {
        void Add(Request request);
        Request Get(string callId);  
    }
}