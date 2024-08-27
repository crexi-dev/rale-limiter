using System;
using RateLimiter.Configuration;
using RateLimiter.Rules;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using RateLimiter.Interface;

namespace RateLimiter.Extensions
{
    /// <summary>
    /// Helper class to ease creation of various rate limiting rules and composite rules
    /// </summary>
    public static class RateLimitRuleHelper
    {
        /// <summary>
        /// Helper to create a fixed window rate limit rule
        /// </summary>
        /// <param name="loggerFactory">The logger factory to use</param>
        /// <param name="memoryCache">The memory cache to use</param>
        /// <param name="fixedWindowRateLimitRuleConfiguration">The configuration to use to create the rate limit rule</param>
        /// <returns>The created rate limit rule </returns>
        public static IRateLimitRule CreateFixedWindowRateLimitRule(ILoggerFactory? loggerFactory, IMemoryCache? memoryCache,
                                                FixedWindowRateLimitRuleConfiguration? fixedWindowRateLimitRuleConfiguration)
        {
            return new FixedWindowRateLimitRule(loggerFactory, memoryCache, fixedWindowRateLimitRuleConfiguration);
        }

        /// <summary>
        /// Helper to create a string match rate limit rule
        /// </summary>
        /// <param name="loggerFactory">The logger factory to use</param>
        /// <param name="fixedWindowRateLimitRuleConfiguration">The configuration to use to create the rate limit rule</param>
        /// <returns>The created rate limit rule </returns>
        public static IRateLimitRule CreateStringMatchRateLimitRule(ILoggerFactory? loggerFactory,
                        StringMatchRateLimitRuleConfiguration? stringMatchRateLimitRuleConfiguration)
        {
            return new StringMatchRateLimitRule(loggerFactory, stringMatchRateLimitRuleConfiguration);
        }

        /// <summary>
        /// Helper to create a composite rate limit rule that combines the behavior of the constituent rate limit rules
        /// </summary>
        /// <param name="loggerFactory">The logger factory to use</param>
        /// <returns>The created composite rate limit rule </returns
        public static ICompositeRateLimitRule CreateCompositeRateLimitRule(ILoggerFactory? loggerFactory)
        {
            return new CompositeRateLimitRule(loggerFactory);
        }

        /// <summary>
        /// Helper to create a composite rate limit rule that combines the behavior of the constituent rate limit rules
        /// </summary>
        /// <param name="loggerFactory">The logger factory to use</param>
        /// <param name="rateLimitRules">The collection of rate limit rules to use for the combined behavior</param>
        /// <returns>The created composite rate limit rule </returns 
        public static ICompositeRateLimitRule CreateCompositeRateLimitRule(ILoggerFactory loggerFactory, List<IRateLimitRule> rateLimitRules)
        {
            if (rateLimitRules is null)
            {
                throw new ArgumentNullException(nameof(rateLimitRules));
            }
            if (rateLimitRules.Count <= 0)
            {
                throw new ArgumentException("List of rate limit rules should contain at least two rules");
            }
            ICompositeRateLimitRule compositeRateLimitRule = CreateCompositeRateLimitRule(loggerFactory);
            foreach (IRateLimitRule rateLimitRule in rateLimitRules)
            {
                compositeRateLimitRule.Add(rateLimitRule);
            }
            return compositeRateLimitRule;
        }

    }
}

