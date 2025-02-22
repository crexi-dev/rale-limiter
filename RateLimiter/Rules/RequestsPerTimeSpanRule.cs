using Microsoft.Extensions.Logging;
using RateLimiter.Models;
using RateLimiter.Stores;
using System;
using System.Threading.Tasks;

namespace RateLimiter.Rules
{
    public class RequestsPerTimeSpanRule : BaseRateLimitRule
    {
        private readonly IRateLimitDataStore<RateLimitCounterModel> _store;
        private readonly TimeSpan _interval;
        private readonly int _numberOfRequestsAllowed;

        public RequestsPerTimeSpanRule(
            int numberOfRequests, 
            TimeSpan interval, 
            IRateLimitDataStore<RateLimitCounterModel> store,
            ILogger<RequestsPerTimeSpanRule> logger)
            : base(numberOfRequests, logger)
        {
            _interval = interval;
            _numberOfRequestsAllowed = numberOfRequests;
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
                limitCounterModel.RequestCount++;
            }
            else
            {
                limitCounterModel.RequestCount = 1;
                limitCounterModel.RequestTime = currentRequestTime;
            }

            _store.Update(request.RequestPath, limitCounterModel);

            if (limitCounterModel.RequestCount > _numberOfRequestsAllowed)
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        private bool IsWithinTimeInterval(long currentRequestTime, long initialRequestTime)
        {
            return currentRequestTime - initialRequestTime < _interval.Ticks;
        }
    }
}
