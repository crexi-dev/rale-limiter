using System.Threading;
using System.Threading.Tasks;
using RateLimiter.Models;

namespace RateLimiter.Rules
{
    public abstract class BaseRateLimitRule : IRateLimitRule
    {
        private const int MINIMUM_CONCURRENT_REQUESTS = 1;
        private readonly SemaphoreSlim _semaphoreSlim;

        public BaseRateLimitRule()
        {
            _semaphoreSlim = new SemaphoreSlim(MINIMUM_CONCURRENT_REQUESTS, MINIMUM_CONCURRENT_REQUESTS);
        }

        public BaseRateLimitRule(int numberOfRequests)
        {
            _semaphoreSlim = new SemaphoreSlim(numberOfRequests, numberOfRequests);
        }

        public async Task<bool> IsRequestAllowedAsync(RequestModel request)
        {
            await _semaphoreSlim.WaitAsync();
            try
            {
                return await ProcessRuleAsync(request);
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        protected abstract Task<bool> ProcessRuleAsync(RequestModel request);
    }
}
