using RateLimiter.Model;

namespace RateLimiter.Interface.Rule
{
    public interface IRateLimiterRule
    {
        List<string> SupportedRegion { get; }
        bool VerifyAccess(Request request);
    }
}