using RateLimitingLibrary.Core.Interfaces;
using RateLimitingLibrary.Core.Models;

namespace RateLimitingLibrary.Rules
{
    /// <summary>
    /// Base class for implementing rate limit rules.
    /// </summary>
    public abstract class BaseRateLimitRule : IRateLimitRule
    {
        public abstract RateLimitResult Evaluate(ClientRequest request);
    }
}