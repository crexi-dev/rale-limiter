using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RateLimiter.Interfaces;
using RateLimiter.Services.Factory;
using RateLimiter.Services.Interfaces;
using RateLimiter.Services.Rule;

namespace RateLimiter
{
    /// <summary>
    /// RateLimiter class
    /// </summary>
    public class RateLimiter : IRateLimiter
    {
        private readonly ILogger _logger;
        private readonly IRuleService _ruleService;

        /// <summary>
        /// Pass rule service instance
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="ruleService"></param>
        public RateLimiter(ILogger<RateLimiter> logger, IRuleService ruleService)
        {
            _logger = logger;
            _ruleService = ruleService;
        }

        /// <summary>
        /// Pass Rule Options, which will be used by factory to create a rule service
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="ruleOptions"></param>
        public RateLimiter(ILogger<RateLimiter> logger, RuleOptions ruleOptions)
        {
            _logger = logger;
            _ruleService = ServiceFactory.CreateRuleService(ruleOptions);
        }

        /// <summary>
        /// Call this periodically to delete older request logs
        /// </summary>
        /// <param name="pastHours"></param>
        /// <returns></returns>
        public async Task ClearOldRequestLogs(int pastHours = 1)
        {
            await _ruleService.DeleteOldRequestLogs(pastHours);
        }

        /// <summary>
        /// Check wheather given user token and resource access is allowed 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        public async Task<bool> IsRequestAllowed(string token, string resourceName)
        {
            _logger.LogInformation($"Checking resource rate limit for {token}\r\n" +
                $"Resource - {resourceName}");

            if (await _ruleService.HasRateLimitExceeded(token, resourceName))
            {
                _logger.LogWarning($"Rate limit exceeded for {token}.");

                return false;
            }

            return true;
        }
    }
}
