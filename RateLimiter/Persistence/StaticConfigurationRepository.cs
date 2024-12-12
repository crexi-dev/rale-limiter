using RateLimiter.Enums;
using RateLimiter.Models;
using System.Collections.Generic;

namespace RateLimiter.Persistence;

internal class StaticConfigurationRepository : IProvideAccessToConfigurationData
{
    // TODO: This is hard-coded to avoid using a database for the assessment, but would actually be persisted data, preferably somewhere like RedisCache
    public int GetNumberOfSecondsToKeepRequestsByKey(string key)
    {
        return key switch
        {
            "abcdefg_/WeatherForecast" => 30,
            "hijklmn_/WeatherForecast" => 10,
            "opqrstu_/WeatherForecast" => 45,
            _ => 60
        };
    }

    // TODO: This is hard-coded to avoid using a database for the assessment, but would actually be persisted data, preferably somewhere like RedisCache
    public List<RateLimitRuleConfiguration> GetRuleConfigurationsByKey(string key)
    {
        return key switch
        {
            "abcdefg_/WeatherForecast" =>
            [
                new RateLimitRuleConfiguration
                {
                    TooManyInTimeSpanRateLimitRuleMaximumRequestsInTimeSpan = 10,
                    TooManyInTimeSpanRateLimitRuleTimeSpanInSeconds = 10,
                    Type = RateLimitRules.TooManyInTimeSpan
                }
            ],
            "hijklmn_/WeatherForecast" =>
            [
                new RateLimitRuleConfiguration
                {
                    TooCloseToLastRequestRateLimitRuleMinimumTimeBetweenRequestsInSeconds = 1,
                    Type = RateLimitRules.TooCloseToLastRequest
                }
            ],
            "opqrstu_/WeatherForecast" =>
            [
                new RateLimitRuleConfiguration
                {
                    TooManyInTimeSpanRateLimitRuleMaximumRequestsInTimeSpan = 10,
                    TooManyInTimeSpanRateLimitRuleTimeSpanInSeconds = 10,
                    Type = RateLimitRules.TooManyInTimeSpan
                },
                new RateLimitRuleConfiguration
                {
                    TooCloseToLastRequestRateLimitRuleMinimumTimeBetweenRequestsInSeconds= 1,
                    Type = RateLimitRules.TooCloseToLastRequest
                }
            ],
            _ => [],
        };
    }
}