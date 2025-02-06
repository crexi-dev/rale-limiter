using Microsoft.Extensions.Logging;
using RateLimiter.Domain.Models;
using System;
using System.Collections.Generic;

namespace RateLimiter.Infrastructure
{
    public class RequestRepository : Domain.Interfaces.IRequestReqository
    {

        private Dictionary<Guid, RateLimiterStats> RateLimiterCache = new Dictionary<Guid, RateLimiterStats>();
        private readonly ILogger<RequestRepository> _logger;
        public RequestRepository(ILogger<RequestRepository> logger)
        {
            _logger = logger;
        }

        public RateLimiterStats GetRateLimiter(Guid id)
        {
            RateLimiterStats rateLimiterStats;
            try
            {
                var exists = RateLimiterCache.TryGetValue(id, out rateLimiterStats);

                if (exists) { return rateLimiterStats; }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return null;
        }

        public void SaveRateLimiter(RateLimiterStats rateLimiterStats)
        {
            try
            {
                var exists = RateLimiterCache.ContainsKey(rateLimiterStats.Id);
                if (exists) { RateLimiterCache.Remove(rateLimiterStats.Id); }
                RateLimiterCache.TryAdd(rateLimiterStats.Id, rateLimiterStats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
    }
}
