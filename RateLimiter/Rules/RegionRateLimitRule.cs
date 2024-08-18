using System;
using System.Threading.Tasks;
using RateLimiter.Enums;
using RateLimiter.Interfaces;

namespace RateLimiter.Rules
{
    public class RegionRateLimitRule : IRateLimitRule
    {
        private readonly IRateLimitRule _usRateLimiter;
        private readonly IRateLimitRule _euRateLimiter;

        public RegionRateLimitRule(int US_maxRequests, TimeSpan US_timeSpan, TimeSpan EU_timeSpan)
        {
            _usRateLimiter = new RequestsPerTimeSpanRule(US_maxRequests, US_timeSpan);
            _euRateLimiter = new TimeSpanRule(EU_timeSpan);
        }

        public async Task<bool> IsRequestAllowedAsync(string accessToken, DateTime requestTime, Region region)
        {
            switch (region)
            {
                case Region.US:
                    return await _usRateLimiter.IsRequestAllowedAsync(accessToken, requestTime, region);
                case Region.EU:
                    return await _euRateLimiter.IsRequestAllowedAsync(accessToken, requestTime, region);
                default:
                    throw new ArgumentException("Invalid region specified"); // Region.ALL_REGIONS should error - a region based rule will expect a proper region defined
            }
        }

        public Task RecordRequest(string accessToken, DateTime requestTime, Region region)
        {
            switch (region)
            {
                case Region.US:
                    return _usRateLimiter.RecordRequest(accessToken, requestTime, region);
                case Region.EU:
                    return _euRateLimiter.RecordRequest(accessToken, requestTime, region);
                default:
                    throw new ArgumentException("Invalid region specified"); // Region.ALL_REGIONS should error - a region based rule will expect a proper region defined
            }
        }
    }
}
