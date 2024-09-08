using RateLimiter.Repositories;
using RateLimiter.Service.Interface;

namespace RateLimiter.Service
{
    // Rule A: X requests per timespan.
    public class RuleA : IRule
    {
        public bool Allow(string resource, string token, DateTime requestTime)
        {
            var setting = RuleASettings.GetSetting(resource);
            var timeSpan = requestTime.AddSeconds(-setting.TimeSpanSecs);
            var requests = RequestsData.GetRequests(token)?.Where(x => x.Resource == resource && x.RequestTime > timeSpan);

            if (requests?.ToList()?.Count <= setting.Requests)
            {
                return true;
            }

            return false;
        }

        public void Configure(string resource, int requests, int timeSpanSecs)
        {
            RuleASettings.SaveSetting(resource, requests, timeSpanSecs);
        }
    }
}
