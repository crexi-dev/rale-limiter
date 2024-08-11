using RateLimiter.Model;

namespace RateLimiter.Interface
{
    public interface IRateLimiterRepository
    {
        Request Update(RequestDTO requestDTO);
        Request Get(RequestDTO requestDTO)
    }
}