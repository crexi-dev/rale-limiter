using System;
using RateLimiter.Configuration;
using System.Threading;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace RateLimiter.Rules
{
    /// <summary>
    /// Behavior of the Fixed Window Rate Limit rule
    /// </summary>
    public class FixedWindowRateLimitRule : AbstractRateLimitRule
    {
        private readonly ILogger _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly FixedWindowRateLimitRuleConfiguration _fixedWindowRateLimitRuleConfiguration;
        private static readonly string s_ruleNamespace = "fwrlr";

        public FixedWindowRateLimitRule(ILoggerFactory? loggerFactory, IMemoryCache? memoryCache,
                                        FixedWindowRateLimitRuleConfiguration? fixedWindowRateLimitRuleConfiguration)
        {
            if (loggerFactory is null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }
            if (memoryCache is null)
            {
                throw new ArgumentNullException(nameof(memoryCache));
            }
            if (fixedWindowRateLimitRuleConfiguration is null)
            {
                throw new ArgumentNullException(nameof(fixedWindowRateLimitRuleConfiguration));
            }
            if (fixedWindowRateLimitRuleConfiguration.Limit <= 0)
            {
                throw new ArgumentException("Limit should be greater than 0");
            }
            if (fixedWindowRateLimitRuleConfiguration.Window <= TimeSpan.Zero)
            {
                throw new ArgumentException("Timespan should be greater than 0");
            }

            _logger = loggerFactory.CreateLogger<FixedWindowRateLimitRule>();
            _memoryCache = memoryCache;
            _fixedWindowRateLimitRuleConfiguration = fixedWindowRateLimitRuleConfiguration;
        }

        /// <summary>
        /// Evaluates the criteria for the fixed window rule and allows or denies the request to go through further processing
        /// </summary>
        /// <param name="clientKey">The value of the client specified key to use for the rule evaluation</param>
        /// <returns>True if the request is allowed, otherwise false if the request is denied</returns>
        public override bool Evaluate(string clientKey)
        {
            if (string.IsNullOrEmpty(clientKey))
            {
                _logger.LogError("client key cannot be empty or null");
                throw new ArgumentException(nameof(clientKey));
            }

            string cacheKey = $"{s_ruleNamespace}_{clientKey}";
            int count = _memoryCache.Get<int>(cacheKey);
            if (count >= _fixedWindowRateLimitRuleConfiguration.Limit)
            {
                _ = Interlocked.Increment(ref _denied);
                _logger.LogWarning($"Rate limiting denied client with key {clientKey}");
                return false;
            }
            _ = _memoryCache.Set(cacheKey, count + 1, _fixedWindowRateLimitRuleConfiguration.Window);
            _ = Interlocked.Increment(ref _allowed);
            return true;
        }
    }
}

