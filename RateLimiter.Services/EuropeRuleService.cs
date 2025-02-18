using RateLimiter.DataStore.Entities;
using RateLimiter.DataStore.Interfaces;
using RateLimiter.Services.Interfaces;
using RateLimiter.Services.Rule;

namespace RateLimiter.Services
{
    public class EuropeRuleService : IRuleService
    {
        private readonly IRateLimitRepository _rateLimitRepository;
        private readonly RuleOptions _ruleOptions;

        public EuropeRuleService(RuleOptions ruleOptions, IRateLimitRepository rateLimitRepository)
        {
            _rateLimitRepository = rateLimitRepository;
            _ruleOptions = ruleOptions;
        }

        public async Task DeleteOldRequestLogs(int pastHours)
        {
            await _rateLimitRepository.DeleteExpiredLogs(pastHours);
        }

        public async Task<bool> HasRateLimitExceeded(string token, string resourceName)
        {
            var currentLogs = await _rateLimitRepository.GetLogsWithinTimeSpan(
                token,
                resourceName,
                _ruleOptions.TimeSpan);

            if (currentLogs.Count > _ruleOptions.MaxCounts ||
                currentLogs.Sum(x=>x.AccessCounts) > _ruleOptions.MaxCounts)
            {
                return true;
            }
           
            _ = _rateLimitRepository.AddRequestLog(new RequestLog()
            {
                ClientToken = token,
                ResourceName = resourceName,
                TimeStampString = DateTime.UtcNow.ToString(_ruleOptions.TimeStampFormat)
            });

            return false;
        }
    }
}
