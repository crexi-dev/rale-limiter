using RateLimiter.Models;
using System.Collections.Generic;

namespace RateLimiter.Persistence;

public interface IProvideAccessToConfigurationData
{
    int GetNumberOfSecondsToKeepRequestsByKey(string key);

    List<RateLimitRuleConfiguration> GetRuleConfigurationsByKey(string key);
}