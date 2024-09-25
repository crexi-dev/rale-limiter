using RateLimiter.Contracts;

namespace RateLimiter.Rules;

public interface IRateRule : IContextualRule<AllowRequestResult, RequestDetails>, IOverridableRule<AllowRequestResult, RequestDetails>
{
}