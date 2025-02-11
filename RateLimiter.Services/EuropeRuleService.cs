using RateLimiter.DataStore.Entities;
using RateLimiter.DataStore.Interfaces;
using RateLimiter.Services.Interfaces;
using RateLimiter.Services.Rule;

namespace RateLimiter.Services
{
    public class EuropeRuleService : IRuleService
    {
        private readonly IDataContext _dataContext;
        private readonly RuleOptions _ruleOptions;

        public EuropeRuleService(RuleOptions ruleOptions, IDataContext dataContext)
        {
            _dataContext = dataContext;
            _ruleOptions = ruleOptions;
        }

        public async Task DeleteOldRequestLogs(int pastHours)
        {
            await _dataContext.DeleteExpiredLogs(pastHours);
        }

        public async Task<bool> HasRateLimitExceeded(string token, string resourceName)
        {
            var currentLogs = await _dataContext.GetLogsWithinTimeSpan(
                token,
                resourceName,
                _ruleOptions.TimeSpan);

            if (currentLogs.Count > _ruleOptions.MaxCounts ||
                currentLogs.Sum(x=>x.AccessCounts) > _ruleOptions.MaxCounts)
            {
                return true;
            }
           
            _ = _dataContext.AddRequestLog(new RequestLog()
            {
                ClientToken = token,
                ResourceName = resourceName,
                TimeStampString = DateTime.UtcNow.ToString(_ruleOptions.TimeStampFormat)
            });

            return false;
        }
    }
}
