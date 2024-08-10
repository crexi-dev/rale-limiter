using RateLimiter.Model;

namespace RateLimiter.Interface
{
    public interface IAccessValidator
    {
        bool Validate(Request request);
    }
}
