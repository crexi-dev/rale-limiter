using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RateLimiter.Dto;
using RateLimiter.Models;
using RateLimiter.Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Services
{
    public class RateLimiterService : IRateLimiterService
    {   
        private readonly IResourceVisitLogService _resourceVisitLogService;
        private readonly IResourceRuleService _resourceRuleService;
        private static readonly object _locker = new object();
        public RateLimiterService(IResourceVisitLogService resourceVisitLogService,
                                  IResourceRuleService resourceRuleService) 
        {
            _resourceVisitLogService = resourceVisitLogService;
            _resourceRuleService = resourceRuleService;
        }

        public async Task<bool> AllowAPIRequest(AccessToken token)
        {
            try
            {
                string endpoint = token.EndpointUrl;
                string sessionId = token.SessionId;

                lock (_locker)
                {
                    var result = DoesRequestSatisfyResourceRules(endpoint, sessionId);
                    if (!result.Item2)
                    {
                        return false;
                    }

                    _resourceVisitLogService.AddResourceVisitLog(new ResourceVisitLog()
                    {
                        ResourceId = result.Item1,
                        SessionId = sessionId,
                        visitTime = DateTime.UtcNow,
                    });
                }
                return true;
            }
            catch (Exception ex) 
            {
                throw ex;
            }
        }

        private (int, bool) DoesRequestSatisfyResourceRules(string endpoint, string sessionId)
        {
            var visitLogs = GetResourceVisitLog(endpoint, sessionId).GetAwaiter().GetResult();
            if(visitLogs.Item1 == 0 || visitLogs.Item2 == null || !visitLogs.Item2.Any()) {
                return (0, true);
            }

            var rules = GetRulesForResource(visitLogs.Item1).GetAwaiter().GetResult();
            return (visitLogs.Item1, IsRequestWithinRules(visitLogs.Item2.OrderByDescending(x => x.visitTime), rules));
        }

        public bool IsRequestWithinRules(IEnumerable<ResourceVisitLog> logs,  IEnumerable<Rule> rules)
        {
            foreach (var rule in rules)
            {
                if (rule.NumberOfRequestAllowedPerIntervalActive.HasValue && rule.NumberOfRequestAllowedPerIntervalActive.Value)
                {
                    var startTime = DateTime.UtcNow.AddSeconds(-rule.Interval);
                    var NumberOfVisits = logs.Where(x => x.visitTime >= startTime && x.visitTime <= DateTime.UtcNow).Count();
                    if(NumberOfVisits >= rule.NumberOfRequestsAllowedPerInterval) 
                    {
                        return false;
                    }
                }
                if (rule.TimeSinceLastRequestActive.HasValue && rule.TimeSinceLastRequestActive.Value)
                {
                    var duration = DateTime.UtcNow - logs.OrderByDescending(x => x.visitTime).ToList().First().visitTime;

                    if(duration.Seconds < rule.TimeSinceLastRequest)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        public async Task<Tuple<int, IEnumerable<ResourceVisitLog>>> GetResourceVisitLog(string endpoint, string sessionId)
        {
            return await _resourceVisitLogService.GetResourceVisitLog(endpoint, sessionId);
        }

        public async Task<IEnumerable<Rule>> GetRulesForResource(int resourceId)
        {
            return await _resourceRuleService.GetRulesForResource(resourceId);
        }
    }
}
