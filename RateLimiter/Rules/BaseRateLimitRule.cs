using System.Threading;
using System.Threading.Tasks;
using RateLimiter.Models;

namespace RateLimiter.Rules
{
    public abstract class BaseRateLimitRule : IRateLimitRule
    {
        private readonly SemaphoreSlim _semaphoreSlim;

        public BaseRateLimitRule(int numberOfRequests)
        {
            _semaphoreSlim = new SemaphoreSlim(numberOfRequests);
        }

        public async Task<bool> IsWithinLimitAsync(RequestModel request)
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
