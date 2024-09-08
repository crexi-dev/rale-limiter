using RateLimiter.Repositories;
using RateLimiter.Service.Interface;

namespace RateLimiter.Service
{
    // Rule B: X Timespan has passed since the last call.
    public class RuleB : IRule
    {
        public bool Allow(string resource, string token, DateTime requestTime)
        {
            var setting = RuleBSettings.GetSetting(resource);
            var timeSpan = requestTime.AddSeconds(-setting.TimeSpanSecs);
            var requests = RequestsData.GetRequests(token)?.Where(x => x.Resource == resource && x.RequestTime > timeSpan);

            if (requests == null || requests.ToList().Count <= 1)
            {
                return true;
            }

            return false;
        }

        public void Configure(string resource, int timeSpanSecs)
        {
            RuleBSettings.SaveSetting(resource, timeSpanSecs);
        }
    }
}
