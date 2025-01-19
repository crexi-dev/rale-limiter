using RateLimiter.Providers;
using RateLimiter.Rules;
using RateLimiter.Store;
using System.Collections.Generic;

namespace RateLimiter.Configuration
{
    public interface IRateLimiterConfiguration
    {
        IDataStore DataStore { get; }

        IDateTimeProvider DateTimeProvider { get; }

        void AddRule(string uri, IRateLimiterRule rateLimiterRule);

        IEnumerable<IRateLimiterRule> GetRules(string uri);
    }
}
