using RateLimiter.Model;

namespace RateLimiter.Interface
{
    public interface IRequestLimitValidator
    {
        bool Validate(Request request);
    }
}
