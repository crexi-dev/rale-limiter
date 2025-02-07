using RateLimiter.Domain.Interfaces;
using RateLimiter.Domain.Models;
using System;
using System.Threading.Tasks;

namespace RateLimiter.Domain.Rules
{
    public class RequestsPerTimespanRule : IRule
    {
        public async Task<Models.RulesResult> ExecuteRule (RateLimiterRequest request, Configurations configurations, RateLimiterStats rateLimiterStats)
        {
            RulesResult result = new RulesResult();

            if (rateLimiterStats.NumberOfRequestsInTimespan < configurations.RequestsPerTimespan && (request.RequestDate - rateLimiterStats.LastRequestDateTime).TotalSeconds < configurations.TimespanSinceLastCall)
            {
                result.Status = true;
                result.Message = "";
                result.updatedRateLimiterStats = new RateLimiterStats
                {
                    Id = request.Id,
                    NumberOfRequestsInTimespan = rateLimiterStats.NumberOfRequestsInTimespan + 1,
                    LastRequestDateTime = rateLimiterStats.LastRequestDateTime
                };
            }
            else if (rateLimiterStats.NumberOfRequestsInTimespan <= configurations.RequestsPerTimespan && (request.RequestDate - rateLimiterStats.LastRequestDateTime).TotalSeconds > configurations.TimespanSinceLastCall )
            {
                result.Status = true;
                result.Message = "";
                result.updatedRateLimiterStats = new RateLimiterStats
                {
                    Id = request.Id,
                    LastRequestDateTime = request.RequestDate,
                    NumberOfRequestsInTimespan = 1
                };
            }
            else 
            { 
                result.Status = false;
                result.Message = "To many requests for timespan.";
                result.RetryAfter = configurations.TimespanSinceLastCall + (int)(request.RequestDate - rateLimiterStats.LastRequestDateTime).TotalSeconds;
                result.updatedRateLimiterStats = new RateLimiterStats
                {
                    Id = request.Id,
                    LastRequestDateTime = rateLimiterStats.LastRequestDateTime,
                    NumberOfRequestsInTimespan = rateLimiterStats.NumberOfRequestsInTimespan
                };
            }

            return result;
        }
    }
}
