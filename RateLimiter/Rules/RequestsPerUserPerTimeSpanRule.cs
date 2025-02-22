using Microsoft.Extensions.Logging;
using RateLimiter.Models;
using RateLimiter.Stores;
using System;
using System.Threading.Tasks;

namespace RateLimiter.Rules
{
    public class RequestsPerUserPerTimeSpanRule : BaseRateLimitRule
    {
        private readonly IRateLimitDataStore<RateLimitCounterModel> _store;
        private readonly TimeSpan _interval;
        private readonly int _numberOfRequests;

        public RequestsPerUserPerTimeSpanRule(
            int numberOfRequests, 
            TimeSpan interval, 
            IRateLimitDataStore<RateLimitCounterModel> store,
            ILogger<RequestsPerUserPerTimeSpanRule> logger)
            : base(numberOfRequests, logger)
        {
            _interval = interval;
            _numberOfRequests = numberOfRequests;
            _store = store;
        }

        protected override Task<bool> ProcessRuleAsync(RequestModel request)
        {
            var currentRequestTime = DateTime.UtcNow.Ticks;
            var limitCounterModel = _store.Get(request.RequestPath);

            if (limitCounterModel == null)
            {
                limitCounterModel = new RateLimitCounterModel(0, currentRequestTime);
                _store.Add(request.RequestPath, limitCounterModel);
            }

            // If the current request is within the timespan, 
            if (IsWithinTimeInterval(currentRequestTime, limitCounterModel.RequestTime))
            {
                limitCounterModel = new RateLimitCounterModel(limitCounterModel.RequestCount + 1, limitCounterModel.RequestTime);

            }

            _store.Update(request.UserId, limitCounterModel);

            if (limitCounterModel.RequestCount > _numberOfRequests)
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        private bool IsWithinTimeInterval(long currentRequestTime, long initialRequestTime)
        {
            return currentRequestTime - initialRequestTime <= _interval.Ticks;
        }
    }
}
