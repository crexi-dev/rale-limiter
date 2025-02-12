using RateLimiter.Abstractions;
using RateLimiter.Config;
using RateLimiter.Enums;
using RateLimiter.Rules;

using System;
using System.Collections.Generic;

namespace RateLimiter;

public class RateLimiterRulesFactory : IProvideRateLimitRules
{
    public IEnumerable<IDefineARateLimitRule> GetRules(RateLimiterConfiguration configuration)
    {
        var rules = new List<IDefineARateLimitRule>();

        // Load rules defined via appSettings
        foreach (var rule in configuration.Rules)
        {
            switch (rule.Type)
            {
                case LimiterType.RequestsPerTimespan:
                    rules.Add(new RequestPerTimespanRule()
                    {
                        Name = rule.Name,
                        Algorithm = rule.Algorithm is null or RateLimitingAlgorithm.Default ? configuration.DefaultAlgorithm : rule.Algorithm.Value,
                        Discriminator = rule.Discriminator,
                        CustomDiscriminatorName = rule.CustomDiscriminatorType,
                        DiscriminatorMatch = rule.DiscriminatorMatch,
                        DiscriminatorKey = rule.DiscriminatorKey,
                        MaxRequests = rule.MaxRequests ?? configuration.DefaultMaxRequests,
                        TimespanMilliseconds = rule.TimespanMilliseconds is null ?
                            TimeSpan.FromMilliseconds(configuration.DefaultTimespanMilliseconds) :
                            TimeSpan.FromMilliseconds(rule.TimespanMilliseconds.Value)
                    });
                    break;
                case LimiterType.TimespanElapsed:
                    rules.Add(new TimespanElapsedRule()
                    {
                        Name = rule.Name,
                        Algorithm = rule.Algorithm is null or RateLimitingAlgorithm.Default ? configuration.DefaultAlgorithm : rule.Algorithm.Value,
                        Discriminator = rule.Discriminator,
                        CustomDiscriminatorName = rule.CustomDiscriminatorType,
                        DiscriminatorMatch = rule.DiscriminatorMatch,
                        DiscriminatorKey = rule.DiscriminatorKey,
                        TimespanSinceMilliseconds = rule.TimespanMilliseconds is null ?
                            TimeSpan.FromMilliseconds(configuration.DefaultTimespanMilliseconds) :
                            TimeSpan.FromMilliseconds(rule.TimespanMilliseconds.Value)
                    });
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        // TODO: Load user-defined rules?  Not sure if we will do that.  User-defined Discriminators, sure - but not a rule ...

        return rules;
    }
}