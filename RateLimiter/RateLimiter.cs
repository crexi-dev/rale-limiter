using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RateLimiter.Abstractions;
using RateLimiter.Config;
using RateLimiter.Enums;
using RateLimiter.Rules;
using RateLimiter.Rules.Algorithms;

using System;
using System.Collections.Generic;
using System.Linq;

namespace RateLimiter;

public class RateLimiter : IRateLimitRequests
{
    private readonly ILogger<RateLimiter> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;

    /// <summary>
    /// List of rules as defined in appSettings.RateLimiter section (or via Fluent registration)
    /// </summary>
    private readonly IEnumerable<IDefineARateLimitRule> _rules;

    private readonly IProvideDiscriminatorValues _discriminatorsProvider;
    private readonly Dictionary<string, IAmARateLimitAlgorithm> _ruleNameAlgorithm;

    public RateLimiter(
        ILogger<RateLimiter> logger,
        IDateTimeProvider dateTimeProvider,
        IOptions<RateLimiterConfiguration> options,
        IProvideRateLimitRules rulesFactory,
        IProvideDiscriminatorValues discriminatorsProvider)
    {
        _logger = logger;
        _dateTimeProvider = dateTimeProvider;
        _rules = rulesFactory.GetRules(options.Value);

        // We need to instantiate an instance of an algorithm for each configuration we find
        // Why? Even though 2 rules might specify the same algo, the config-based specifics could be different
        _ruleNameAlgorithm = GenerateAlgorithmsFromRules(_rules);

        _discriminatorsProvider = discriminatorsProvider;
    }

    public (bool RequestIsAllowed, string ErrorMessage) IsRequestAllowed(
        HttpContext context,
        IEnumerable<RateLimitedResource> rateLimitedResources)
    {
        // get the matching rules for this request
        var matchingRules = _rules.Where(r => rateLimitedResources
            .Select(x => x.RuleName)
            .ToList().Contains(r.Name))
            .ToList();

        if (matchingRules.Count == 0)
        {
            _logger.LogInformation("No match for {@RuleName}", rateLimitedResources.First().RuleName);
            return (true, string.Empty);
        }

        // get the algorithm required for each rule to be evaluated (move to ctor?)
        //var requiredAlgorithms = _ruleNameAlgorithm
        //    .Where(x => matchingRules.Select(y => y.Name)
        //        .Contains(x.Key))
        //    .Select(x => x.Value);

        // need to get the discriminator for each incoming rate limit configuration
        var discriminatorValues = _discriminatorsProvider
            .GetDiscriminatorValues(context, matchingRules); //key: name value: discriminatorValue

        // TODO: Make this a single call (no iterations)
        var passed = true;
        foreach (var rule in matchingRules)
        {
            passed = _ruleNameAlgorithm[rule.Name].IsAllowed(discriminatorValues[rule.Name].ToString());
            if (!passed)
                break;
        }

        // ensure they all pass
        //var passed = algorithms.All(x => x.IsAllowed(discriminators[x.Name].ToString()));

        return passed ? (passed, string.Empty) :
            (passed, "some message about banging on our door too much");
    }

    /// <summary>
    /// From the rules we have configured in appSettings for our rate limiter,
    /// we need to instantiate an algorithm configured to satisfy the rule's configuration
    /// </summary>
    /// <param name="rules"></param>
    /// <returns></returns>
    /// <exception cref="InvalidCastException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private Dictionary<string, IAmARateLimitAlgorithm> GenerateAlgorithmsFromRules(IEnumerable<IDefineARateLimitRule> rules)
    {
        var values = new Dictionary<string, IAmARateLimitAlgorithm>();

        var algorithms = new Dictionary<string, IAmARateLimitAlgorithm>();

        foreach (var rule in rules)
        {
            switch (rule.Type)
            {
                case LimiterType.RequestsPerTimespan:

                    if (rule is not RequestPerTimespanRule typedRule)
                    {
                        throw new InvalidCastException("uh oh");
                    }

                    // do we have an algorithm that meets these requirements?
                    var algoKey = $"{typedRule.Algorithm}|{typedRule.MaxRequests}|{typedRule.TimespanMilliseconds}";

                    if (!algorithms.TryGetValue(algoKey, out var existingAlgo))
                    {
                        // create the required algo with the required config
                        var algo = GetAlgorithm(
                            _dateTimeProvider,
                            typedRule.Algorithm,
                            typedRule.MaxRequests,
                            typedRule.TimespanMilliseconds);
                        values.Add(typedRule.Name, algo);
                        algorithms.Add(algoKey, algo);
                    }
                    else
                    {
                        values.Add(typedRule.Name, existingAlgo);
                    }
                    break;
                case LimiterType.TimespanElapsed:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return values;
    }

    private static IAmARateLimitAlgorithm GetAlgorithm(
        IDateTimeProvider dateTimeProvider,
        RateLimitingAlgorithm algo,
        int? maxRequests,
        TimeSpan? timespanMilliseconds)
    {
        return algo switch
        {
            RateLimitingAlgorithm.Default or RateLimitingAlgorithm.FixedWindow => new FixedWindow(dateTimeProvider,
                new FixedWindowConfiguration()
                {
                    MaxRequests = maxRequests.Value,
                    WindowDuration = timespanMilliseconds.Value
                }),
            RateLimitingAlgorithm.TokenBucket => new TokenBucket(),
            RateLimitingAlgorithm.LeakyBucket => new LeakyBucket(dateTimeProvider, maxRequests.Value, timespanMilliseconds.Value),
            RateLimitingAlgorithm.SlidingWindow => new SlidingWindow(dateTimeProvider, maxRequests.Value, timespanMilliseconds.Value),
            _ => throw new ArgumentOutOfRangeException(nameof(algo), algo, null)
        };
    }
}