using Microsoft.Extensions.Logging;
using RateLimiter.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RateLimiter.Rules
{
    public abstract class BaseRateLimitRule : IRateLimitRule
    {
        private readonly SemaphoreSlim _semaphoreSlim;
        private readonly ILogger _logger;

        public BaseRateLimitRule(int numberOfRequests, ILogger logger)
        {
            _semaphoreSlim = new SemaphoreSlim(numberOfRequests);
            _logger = logger;
        }

        public async Task<bool> IsWithinLimitAsync(RequestModel request)
        {
            await _semaphoreSlim.WaitAsync();
            try
            {
                return await ProcessRuleAsync(request);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
            finally
            {
                _semaphoreSlim.Release();
            }

            return true;
        }

        protected abstract Task<bool> ProcessRuleAsync(RequestModel request);
    }
}
