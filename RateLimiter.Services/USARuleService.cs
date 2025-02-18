﻿using RateLimiter.DataStore.Entities;
using RateLimiter.DataStore.Interfaces;
using RateLimiter.Services.Interfaces;
using RateLimiter.Services.Rule;

namespace RateLimiter.Services
{
    public class USARuleService : IRuleService
    {
        private readonly IRateLimitRepository _rateLimitRepository;
        private readonly RuleOptions _ruleOptions;

        public USARuleService(RuleOptions ruleOptions, IRateLimitRepository rateLimitRepository) 
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
            var timeStampString = DateTime.UtcNow.ToString(_ruleOptions.TimeStampFormat);

            var currentLog = await _rateLimitRepository.GetLogByTimeStamp(
                token,
                resourceName,
                timeStampString);

            if (currentLog is { })
            {
                if (currentLog.AccessCounts >= _ruleOptions.MaxCounts)
                {
                    return true;
                }

                _ = _rateLimitRepository.UpdateRequestLogCounts(currentLog);
            }
            else
            {
                _ = _rateLimitRepository.AddRequestLog(new RequestLog()
                {
                    ClientToken = token,
                    ResourceName = resourceName,
                    TimeStampString = timeStampString,
                });
            }

            return false;
        }
    }
}
