using RateLimitingLibrary.Core.Models;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace RateLimitingLibrary.Rules
{
    /// <summary>
    /// Implements a sliding window rate limiting rule.
    /// </summary>
    public class SlidingWindowRule : BaseRateLimitRule
    {
        private readonly int _limit;
        private readonly TimeSpan _window;
        private readonly ConcurrentDictionary<string, ConcurrentQueue<DateTime>> _requestTimestamps = new();

        public SlidingWindowRule(int limit, TimeSpan window)
        {
            _limit = limit;
            _window = window;
        }

        public override RateLimitResult Evaluate(ClientRequest request)
        {
            var now = request.RequestTime;
            var timestamps = _requestTimestamps.GetOrAdd(request.ClientToken, _ => new ConcurrentQueue<DateTime>());

            lock (timestamps)
            {
                while (timestamps.TryPeek(out var oldest) && now - oldest > _window)
                {
                    timestamps.TryDequeue(out _);
                }

                timestamps.Enqueue(now);
                return timestamps.Count > _limit
                    ? new RateLimitResult { IsAllowed = false, Message = "Rate limit exceeded." }
                    : new RateLimitResult { IsAllowed = true };
            }
        }
    }
}