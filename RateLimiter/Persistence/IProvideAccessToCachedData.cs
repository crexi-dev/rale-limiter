using RateLimiter.Models;
using System;
using System.Collections.Generic;

namespace RateLimiter.Persistence;

public interface IProvideAccessToCachedData
{
    void AddRequestByKey(string key);

    List<DateTime> GetRequestsByKey(string key);

    List<RateLimitRuleConfiguration> GetRuleConfigurationsByKey(string key);

    void Lock(string key);

    void RemoveOldRequests(string key);

    void Unlock(string key);
}