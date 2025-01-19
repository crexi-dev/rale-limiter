using RateLimiter.Configuration;
using RateLimiter.Providers;
using RateLimiter.Rules;
using RateLimiter.Store;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RateLimiter.Configuration
{
    public class RateLimiterConfiguration : IRateLimiterConfiguration
    {
        private readonly IDataStore _dataStore;
        private readonly IDateTimeProvider _dateTimeProvider;

        private readonly Dictionary<string, List<IRateLimiterRule>> _ruleMap = [];

        public RateLimiterConfiguration(IDataStore dataStore, IDateTimeProvider dateTimeProvider)
        {
            ArgumentNullException.ThrowIfNull(nameof(dataStore));
            ArgumentNullException.ThrowIfNull(nameof(dateTimeProvider));
            _dateTimeProvider = dateTimeProvider;
            _dataStore = dataStore;
        }

        public IDataStore DataStore { get { return _dataStore; } }

        public IDateTimeProvider DateTimeProvider { get { return _dateTimeProvider; } }

        public void AddRule(string uri, IRateLimiterRule rateLimiterRule)
        {
            if (_ruleMap.ContainsKey(uri))
            {
                _ruleMap[uri].Add(rateLimiterRule);
                return;
            }

            _ruleMap.Add(uri, [rateLimiterRule]);
        }

        public IEnumerable<IRateLimiterRule> GetRules(string uri)
        {
            return _ruleMap.TryGetValue(uri, out var rules) ? rules : Enumerable.Empty<IRateLimiterRule>();
        }
    }
}
