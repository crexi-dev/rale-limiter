using System;
using System.Threading.Tasks;
using RateLimiter.Models;
using RateLimiter.Stores;

namespace RateLimiter.Rules
{
    public class TimeSpanSinceLastRequestRule : BaseRateLimitRule
    {
        private readonly IDataStoreKeyGenerator _keyGenerator;
        private readonly IRateLimitDataStore _store;
        private readonly TimeSpan _interval;

        public TimeSpanSinceLastRequestRule(
            TimeSpan interval,
            IRateLimitDataStore store,
            IDataStoreKeyGenerator keyGenerator)
        {
            _interval = interval;
            _store = store;
            _keyGenerator = keyGenerator;
        }

        protected override Task<bool> ProcessRuleAsync(RequestModel request)
        {
            var rateLimitDataKey = _keyGenerator.GenerateKey(request);
            var currentRequestTime = DateTime.UtcNow.Ticks;
            var limitCounterModel = _store.Get(rateLimitDataKey);
            var allowRequest = true;

            if (limitCounterModel == null)
            {
                limitCounterModel = new RateLimitCounterModel(0, currentRequestTime);
                _store.Add(rateLimitDataKey, limitCounterModel);
            }

            // If the current request is within the timespan, 
            if (IsWithinTimeInterval(currentRequestTime, limitCounterModel.RequestTime))
            {
                limitCounterModel.RequestCount++;
                if (limitCounterModel.RequestCount > 1)
                {
                    allowRequest = false;
                }
            }
            else
            {
                limitCounterModel.RequestCount = 1;
                limitCounterModel.RequestTime = currentRequestTime;
            }

            _store.Update(rateLimitDataKey, limitCounterModel);

            return Task.FromResult(allowRequest);
        }

        private bool IsWithinTimeInterval(long currentRequestTime, long initialRequestTime)
        {
            return currentRequestTime - initialRequestTime < _interval.Ticks;
        }
    }
}
