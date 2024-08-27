using System;
using System.Linq;
using RateLimiter.Interface;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Data;
using System.Threading;

namespace RateLimiter
{
    /// <summary>
    /// Behavior of the composite Rate Limit rule, one whose behavior is composed by its constituent rules
    /// </summary>
    public class CompositeRateLimitRule : ICompositeRateLimitRule
    {
        private readonly ILogger _logger;
        private readonly ConcurrentBag<IRateLimitRule> _rateLimitRules;

        public CompositeRateLimitRule(ILoggerFactory? loggerFactory)
        {
            if (loggerFactory is null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }
            _logger = loggerFactory.CreateLogger<CompositeRateLimitRule>();
            _rateLimitRules = new ConcurrentBag<IRateLimitRule>();
        }

        /// <summary>
        /// Adds this rate limit rule to its collection of rules to evaluate
        /// </summary>
        /// <param name="rateLimitRule">The rate limit rule to add to the collection</param>
        public void Add(IRateLimitRule? rateLimitRule)
        {
            if (rateLimitRule is null)
            {
                _logger.LogError("rate limit rule cannot be null");
                throw new ArgumentNullException(nameof(rateLimitRule));
            }
            _rateLimitRules.Add(rateLimitRule);
        }

        /// <summary>
        /// Evaluates the criteria using all the composed rules and allows or denies the request to go through further processing
        /// Assuming the same client specified key will be used for all the composed rules 
        /// </summary>
        /// <param name="clientKey">The value of the client specified key to use for the rule evaluation</param>
        /// <returns>True if the request is allowed, otherwise false if the request is denied</returns>
        public bool Evaluate(string clientKey)
        {
            if (string.IsNullOrEmpty(clientKey))
            {
                _logger.LogError("client key cannot be empty or null");
                throw new ArgumentException(nameof(clientKey));
            }

            if (_rateLimitRules.IsEmpty)
            {
                throw new ArgumentException("no rate limit rules have been defined");
            }

            bool allow = true;
            // evaluate all rules to determine the final result. Even if one rule evaluates and denies the request, the final result is denied 
            foreach (IRateLimitRule rateLimitRule in _rateLimitRules)
            {
                if (!rateLimitRule.Evaluate(clientKey))
                {
                    _logger.LogWarning($"Rate limiting denied client with key {clientKey}");
                    allow = false;
                }
            }
            return allow;
        }

        /// <summary>
        /// Gets the count of the requests allowed
        /// </summary>
        public long Allowed => (from rule in _rateLimitRules
                                select rule.Allowed).Sum();

        /// <summary>
        /// Gets the count of the requests denied
        /// </summary>
        public long Denied => (from rule in _rateLimitRules
                               select rule.Denied).Sum();
    }
}

