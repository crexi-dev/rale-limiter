using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RateLimiter.Abstractions;
using RateLimiter.Config;
using RateLimiter.Enums;
using RateLimiter.Rules;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace RateLimiter;

public class RateLimiter : IRateLimitRequests
{
    private readonly ILogger<RateLimiter> _logger;
    private readonly IProvideRateLimitAlgorithms _algorithmProvider;

    /// <summary>
    /// List of rules as defined in appSettings.RateLimiter section (or via Fluent registration)
    /// </summary>
    private readonly IEnumerable<IDefineARateLimitRule> _rules;

    private readonly IProvideDiscriminatorValues _discriminatorsProvider;
    private readonly ConcurrentDictionary<string, IAmARateLimitAlgorithm> _ruleNameAlgorithm;

    public RateLimiter(
        ILogger<RateLimiter> logger,
        IOptions<RateLimiterConfiguration> options,
        IProvideRateLimitRules rulesFactory,
        IProvideDiscriminatorValues discriminatorsProvider,
        IProvideRateLimitAlgorithms algorithmProvider)
    {
        _logger = logger;
        _algorithmProvider = algorithmProvider;

        // TODO: IOptions<RateLimiterConfiguration> should be replaced with IOptionsMonitor<T> for hot-reloading
        _rules = rulesFactory.GetRules(options.Value);

        // We need to instantiate an instance of an algorithm for each configuration we find
        // Why? Even though 2 rules might specify the same algo, the config-based specifics could be different
        _ruleNameAlgorithm = GenerateAlgorithmsFromRules(_rules);

        _discriminatorsProvider = discriminatorsProvider;

        ValidateConfiguration();
    }

    /// <summary>
    /// Validate configuration and registrations upon instantiation in order to prevent downstream runtime errors & exceptions
    /// </summary>
    private void ValidateConfiguration()
    {
        foreach (var rule in _rules)
        {
            switch (rule.Discriminator)
            {
                case LimiterDiscriminator.Custom:
                    if (string.IsNullOrEmpty(rule.CustomDiscriminatorName))
                        throw new MissingFieldException("Rule uses a custom discriminator, but none was provided. {@RuleName}", rule.Name);
                    
                    break;
                case LimiterDiscriminator.GeoLocation:
                case LimiterDiscriminator.IpAddress:
                case LimiterDiscriminator.IpSubNet:
                    break;
                case LimiterDiscriminator.QueryString:
                    if (string.IsNullOrEmpty(rule.DiscriminatorKey))
                        throw new MissingFieldException("Rule uses a querystring discriminator, but DiscriminatorKey was not provided. {@RuleName}", rule.Name);
                    break;
                case LimiterDiscriminator.RequestHeader:
                    if (string.IsNullOrEmpty(rule.DiscriminatorKey))
                        throw new MissingFieldException("Rule uses a request header discriminator, but DiscriminatorKey was not provided. {@RuleName}", rule.Name);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch (rule.Type)
            {
                case LimiterType.RequestsPerTimespan:
                    break;
                case LimiterType.TimespanElapsed:
                    // ensure this is correct; cannot be anything else for this rule type
                    // do not throw an exception, simply correct it
                    if (rule.Algorithm != RateLimitingAlgorithm.TimespanElapsed)
                        rule.Algorithm = RateLimitingAlgorithm.TimespanElapsed;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
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

        // need to get the discriminator for each incoming rate limit configuration
        // key: ruleName value: (IsMatch, MatchValue)
        var discriminatorValues = _discriminatorsProvider
            .GetDiscriminatorValues(context, matchingRules)
            .Where(x => x.Value.IsMatch)
            .ToList();

        // now we need to filter down the matchingRules only to those whose discriminators matched their condition(s)
        matchingRules = matchingRules
            .Where(x => discriminatorValues.Select(y => y.Key)
                .Contains(x.Name))
            .ToList();

        // TODO: Make this a single call (no iterations)
        var passed = true;
        foreach (var rule in matchingRules)
        {
            passed = _ruleNameAlgorithm[rule.Name]
                .IsAllowed(discriminatorValues.First(x => x.Key == rule.Name).Value.MatchValue);
            if (!passed)
                break;
        }

        // TODO: We would want to make this configurable - what status code to use and what we tell the client
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
    private ConcurrentDictionary<string, IAmARateLimitAlgorithm> GenerateAlgorithmsFromRules(IEnumerable<IDefineARateLimitRule> rules)
    {
        var values = new ConcurrentDictionary<string, IAmARateLimitAlgorithm>();

        var algorithms = new ConcurrentDictionary<string, IAmARateLimitAlgorithm>();

        foreach (var rule in rules)
        {
            // TODO: Clean up this mess!
            switch (rule.Type)
            {
                case LimiterType.RequestsPerTimespan:

                    if (rule is not RequestPerTimespanRule typedRule)
                        throw new InvalidCastException("uh oh");

                    // do we have an algorithm that meets these requirements?
                    var rptKey = $"{typedRule.Algorithm}|{typedRule.MaxRequests}|{typedRule.TimespanMilliseconds}";

                    if (!algorithms.TryGetValue(rptKey, out var existingAlgo))
                    {
                        // create the required algo with the required config
                        var algo = _algorithmProvider.GetAlgorithm(
                            typedRule.Algorithm,
                            typedRule.MaxRequests,
                            typedRule.TimespanMilliseconds);
                        values.TryAdd(typedRule.Name, algo);
                        algorithms.TryAdd(rptKey, algo);
                    }
                    else
                    {
                        values.TryAdd(typedRule.Name, existingAlgo);
                    }

                    break;
                case LimiterType.TimespanElapsed:

                    if (rule is not TimespanElapsedRule teRule)
                        throw new InvalidCastException("uh oh");

                    // do we have an algorithm that meets these requirements?
                    var teKey = $"{teRule.Algorithm}|{teRule.TimespanSinceMilliseconds}";

                    if (!algorithms.TryGetValue(teKey, out var existingTeAlgo))
                    {
                        // create the required algo with the required config
                        var algo = _algorithmProvider.GetAlgorithm(
                            teRule.Algorithm,
                            null,
                            teRule.TimespanSinceMilliseconds);
                        values.TryAdd(teRule.Name, algo);
                        algorithms.TryAdd(teKey, algo);
                    }
                    else
                    {
                        values.TryAdd(teRule.Name, existingTeAlgo);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return values;
    }
}