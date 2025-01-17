using RateLimitingLibrary.Core.Models;
using System;
using System.Collections.Concurrent;

namespace RateLimitingLibrary.Rules
{
    /// <summary>
    /// Implements a fixed window rate limiting rule.
    /// </summary>
    public class FixedWindowRule : BaseRateLimitRule
    {
        private readonly int _limit;
        private readonly TimeSpan _window;
        private readonly ConcurrentDictionary<string, (int Count, DateTime WindowStart)> _requestCounts = new();

        public FixedWindowRule(int limit, TimeSpan window)
        {
            _limit = limit;
            _window = window;
        }

        public override RateLimitResult Evaluate(ClientRequest request)
        {
            var windowKey = request.ClientToken;
            var now = request.RequestTime;

            _requestCounts.AddOrUpdate(windowKey,
                _ => (1, now),
                (_, existing) =>
                {
                    if (now - existing.WindowStart > _window)
                        return (1, now);

                    return (existing.Count + 1, existing.WindowStart);
                });

            var current = _requestCounts[windowKey];

            return current.Count > _limit
                ? new RateLimitResult { IsAllowed = false, Message = "Rate limit exceeded." }
                : new RateLimitResult { IsAllowed = true };
        }
    }
}