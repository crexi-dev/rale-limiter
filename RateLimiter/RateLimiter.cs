using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RateLimiter.Abstractions;
using RateLimiter.Config;
using RateLimiter.Discriminators;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace RateLimiter;

public class RateLimiter : IRateLimiter
{
    private readonly ILogger<RateLimiter> _logger;
    private readonly IOptions<RateLimiterConfiguration> _options;
    private readonly RateLimiterConfiguration _config;
    private readonly IRateLimitAlgorithmProvider _algorithmProvider;
    private readonly IRateLimitDiscriminatorProvider _discriminatorsProvider;
    private readonly IRateLimiterConfigurationValidator _configurationValidator;

    private ConcurrentDictionary<string, IRateLimitAlgorithm> _algorithms;
    private ConcurrentDictionary<string, IRateLimitDiscriminator> _discriminators;

    public RateLimiter(
        ILogger<RateLimiter> logger,
        IOptions<RateLimiterConfiguration> options,
        IRateLimitDiscriminatorProvider discriminatorsProvider,
        IRateLimitAlgorithmProvider algorithmProvider,
        IRateLimiterConfigurationValidator configurationValidator)
    {
        _logger = logger;
        _algorithmProvider = algorithmProvider;
        _options = options; // TODO: IOptions<RateLimiterConfiguration> should be replaced with IOptionsMonitor<T> for hot-reloading
        _config = options.Value;
        _discriminatorsProvider = discriminatorsProvider;
        _configurationValidator = configurationValidator;

        ValidateConfiguration(_config);
        ProcessConfiguration(_config);
    }

    private void ValidateConfiguration(RateLimiterConfiguration config)
    {
        var validationResult = _configurationValidator.Validate(config);
        if (!validationResult.IsValid)
            throw new ApplicationException($"RateLimiter Configuration is invalid. {validationResult.Errors.ToList()}");
    }

    private void ProcessConfiguration(RateLimiterConfiguration configuration)
    {
        /* Preload Algorithms */
        // We need to instantiate an instance of an algorithm for each configuration we find
        // Why? Even though 2 rules might specify the same algo, the config-based specifics could be different
        // From the rules we have configured in appSettings for our rate limiter,
        // we need to instantiate an algorithm configured to satisfy the rule's configuration
        _algorithms = _algorithmProvider.GenerateAlgorithmsFromRules(configuration);

        /* Preload Discriminators */
        _discriminators = _discriminatorsProvider.GenerateDiscriminators(_config.Discriminators);
    }

    public (bool RequestIsAllowed, string ErrorMessage) IsRequestAllowed(
        HttpContext context,
        IEnumerable<RateLimitedResource> rateLimitedResources)
    {
        // get the matching rules for this request
        var matchingRules = _config.Rules.Where(r => rateLimitedResources
            .Select(x => x.RuleName)
            .ToList().Contains(r.Name))
            .ToList();

        if (matchingRules.Count == 0)
        {
            _logger.LogInformation("No match for {@RuleName}", rateLimitedResources.First().RuleName);
            return (true, string.Empty);
        }

        // get the discriminator(s) for each rule to be processed for this request
        var discriminatorNames = matchingRules.SelectMany(x => x.Discriminators);

        var discriminatorsToProcess = _discriminators
            .Where(x => discriminatorNames.Contains(x.Key))
            .Select(y => y.Value)
            .ToList();

        if (discriminatorsToProcess.Count == 0)
        {
            // this is likely a logical/configuration error
            return (true, string.Empty);
        }

        // now we need to filter down the matchingRules only to those whose discriminators matched their condition(s)
        var results = new List<DiscriminatorEvaluationResult>();
        discriminatorsToProcess.ForEach(x =>
        {
            results.Add(x.Evaluate(context));
        });

        // lastly, for each of the discriminators, process it with the correct algorithm IFF the discriminator was a match
        var passed = true;
        var lastRule = string.Empty;
        foreach (var x in results.Where(r => r.IsMatch))
        {
            lastRule = $"{x.DiscriminatorName}:{x.AlgorithmName}";
            passed = _algorithms[x.AlgorithmName!].IsAllowed(x.MatchValue);
            if (!passed)
                break;
        }

        // TODO: We would want to make this configurable - what status code to use and what we tell the client
        return passed ? (passed, string.Empty) :
            (passed, $"some message about banging on our door too much due to: {lastRule}");
    }
}