using RateLimitingLibrary.Config;
using RateLimitingLibrary.Core.Interfaces;
using RateLimitingLibrary.Core.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RateLimitingLibrary.Core.Services
{
    /// <summary>
    /// Service for managing rate limiting evaluations for client requests.
    /// </summary>
    public class RateLimiter : IRateLimiter
    {
        private readonly RuleConfigurations _configurations;
        private readonly IEnumerable<IRateLimitRule> _rules;
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores = new();

        public RateLimiter(RuleConfigurations configurations, IEnumerable<IRateLimitRule> rules)
        {
            _configurations = configurations;
            _rules = rules;
        }

        public async Task<RateLimitResult> EvaluateRequestAsync(ClientRequest request)
        {
            if (!_configurations.ResourceRules.TryGetValue(request.Resource, out var ruleNames))
            {
                return new RateLimitResult { IsAllowed = true };
            }

            foreach (var ruleName in ruleNames)
            {
                var rule = _rules.FirstOrDefault(r => r.GetType().Name == ruleName);
                if (rule == null) continue;

                var semaphore = _semaphores.GetOrAdd(request.ClientToken, _ => new SemaphoreSlim(1, 1));

                await semaphore.WaitAsync();
                try
                {
                    var result = rule.Evaluate(request);
                    if (!result.IsAllowed) return result;
                }
                finally
                {
                    semaphore.Release();
                }
            }

            return new RateLimitResult { IsAllowed = true };
        }
    }
}