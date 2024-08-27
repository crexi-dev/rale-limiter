using System;
using RateLimiter.Configuration;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace RateLimiter.Rules
{
    /// <summary>
    /// Behavior of the String Match Rate Limit rule
    /// </summary>
    public class StringMatchRateLimitRule : AbstractRateLimitRule
    {
        private readonly ILogger<StringMatchRateLimitRule> _logger;
        private readonly StringMatchRateLimitRuleConfiguration _stringMatchRateLimitRuleConfiguration;

        public StringMatchRateLimitRule(ILoggerFactory? loggerFactory,
                                        StringMatchRateLimitRuleConfiguration? stringMatchRateLimitRuleConfiguration)
        {
            if (loggerFactory is null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }
            if (stringMatchRateLimitRuleConfiguration is null)
            {
                throw new ArgumentNullException(nameof(stringMatchRateLimitRuleConfiguration));
            }
            if (string.IsNullOrEmpty(stringMatchRateLimitRuleConfiguration.Match))
            {
                throw new ArgumentException("Key to match cannot be empty or null");
            }

            _logger = loggerFactory.CreateLogger<StringMatchRateLimitRule>();
            _stringMatchRateLimitRuleConfiguration = stringMatchRateLimitRuleConfiguration;
        }

        /// <summary>
        /// Evaluates the criteria for the string match rule and allows or denies the request to go through further processing
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

            if (!string.Equals(_stringMatchRateLimitRuleConfiguration.Match, clientKey, StringComparison.OrdinalIgnoreCase))
            {
                _ = Interlocked.Increment(ref _denied);
                _logger.LogWarning($"Rate limiting denied client with key {clientKey} due to mismatch " +
                    $"with {_stringMatchRateLimitRuleConfiguration.Match}");
                return false;
            }
            _ = Interlocked.Increment(ref _allowed);
            return true;
        }
    }
}

