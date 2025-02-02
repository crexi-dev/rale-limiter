using Crexi.RateLimiter.Rule.Model;

namespace Crexi.RateLimiter.Rule.ResourceAccess;

public interface IRateLimitResourceAccess
{
    CallHistory AddCallAndGetHistory(CallData callData);
    IList<RateLimitRule>? GetRules(CallData callData);

    IRateLimitResourceAccess SetExpirationWindow(TimeSpan timespan, CallData callData);
    IRateLimitResourceAccess SetRules(IEnumerable<RateLimitRule> rules, CallData callData);
}