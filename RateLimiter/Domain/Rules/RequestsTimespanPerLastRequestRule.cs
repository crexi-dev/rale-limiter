using RateLimiter.Domain.Interfaces;
using RateLimiter.Domain.Models;
using System.Threading.Tasks;

namespace RateLimiter.Domain.Rules
{
    public class RequestsTimespanPerLastRequestRule : IRule
    {
        public async Task<Models.RulesResult> ExecuteRule(RateLimiterRequest request, Configurations configurations, RateLimiterStats rateLimiterStats)
        {
            RulesResult result = new RulesResult();

            if ((request.RequestDate - rateLimiterStats.LastRequestDateTime).TotalSeconds > configurations.TimespanSinceLastCall)
            {
                result.Status = true;
                result.Message = "";
                result.updatedRateLimiterStats = new RateLimiterStats
                {
                    Id = request.Id,
                    LastRequestDateTime = request.RequestDate
                };
            }
            else
            {
                result.Status = false;
                result.Message = "Timespan has not expired.";
                result.RetryAfter = configurations.TimespanSinceLastCall + (int)(request.RequestDate - rateLimiterStats.LastRequestDateTime).TotalSeconds;
                result.updatedRateLimiterStats = new RateLimiterStats
                {
                    Id = request.Id,
                    LastRequestDateTime = rateLimiterStats.LastRequestDateTime
                };
            }

            return result;
        }
    }
}
