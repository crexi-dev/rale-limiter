using System;
using System.Threading.Tasks;
using RateLimiter.Models;
using RateLimiter.Stores;

namespace RateLimiter.Rules
{
    public class RequestsPerTimeSpanRule : BaseRateLimitRule
    {
        private readonly IDataStoreKeyGenerator _keyGenerator;
        private readonly IRateLimitDataStore _store;
        private readonly TimeSpan _interval;
        private readonly int _numberOfRequestsAllowed;

        public RequestsPerTimeSpanRule(
            int numberOfRequests, 
            TimeSpan interval, 
            IRateLimitDataStore store,
            IDataStoreKeyGenerator keyGenerator)
            : base(numberOfRequests)
        {
            _interval = interval;
            _numberOfRequestsAllowed = numberOfRequests;
            _store = store;
            _keyGenerator = keyGenerator;
        }

        protected override Task<bool> ProcessRuleAsync(RequestModel request)
        {
            var rateLimitDataKey = _keyGenerator.GenerateKey(request);
            var currentRequestTime = DateTime.UtcNow.Ticks;
            var limitCounterModel = _store.Get(rateLimitDataKey);

            if (limitCounterModel == null)
            {
                limitCounterModel = new RateLimitCounterModel(0, currentRequestTime);
                _store.Add(rateLimitDataKey, limitCounterModel);
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

            _store.Update(rateLimitDataKey, limitCounterModel);

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
