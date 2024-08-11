using RateLimiter.Model;

namespace RateLimiter.Interface
{
    public interface IRateLimiterRule
    {
        List<string> SupportedRegion { get; }
        bool VerifyAccess(Request request);
    }
}
