using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RateLimiter.Contracts;
using RateLimiter.Processors;
using RateLimiter.Rules;
using Microsoft.Extensions.Logging;

namespace RateLimiter.Infrastructure;

public class DefaultEngine : IContextualProcessingEngine<IRateRule, AllowRequestResult, RequestDetails>
{
    private readonly ILogger<DefaultEngine> _logger;

    /// <summary>
    /// Represents the default processing engine for rate limiting rules.
    /// </summary>
    public DefaultEngine(ILogger<DefaultEngine> logger)
    {
        _logger = logger;
        _logger.LogInformation("DefaultEngine initialized");
    }

    /// <summary>
    /// Process the rate rules for a given context.
    /// </summary>
    /// <param name="rules">The rate rules to process</param>
    /// <param name="context">The context for which the rules are applied</param>
    /// <returns>An instance of AllowRequestResult indicating if all rules passed and the reason if any rule failed</returns>
    public async Task<AllowRequestResult> ProcessRules(IEnumerable<IRateRule> rules,
        IEnumerable<RequestDetails> context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        ArgumentNullException.ThrowIfNull(rules, nameof(rules));

        var allRulesPass = true;

        var cts = new CancellationTokenSource();
        var tasks = new List<Task<AllowRequestResult>>();

        string failedReason = null;

        try
        {
            foreach (var rule in rules)
            {
                Console.WriteLine($"Processing rule: {rule}");
                var task = Task.Run(async () =>
                {
                    var result = await rule.Evaluate(context);
                    Console.WriteLine($"Rule Completed: {rule}");
                    if (result.AllowRequest) return result;

                    allRulesPass = false;
                    failedReason = result.Reason;
                    await cts.CancelAsync();

                    return result;
                }, cts.Token);

                tasks.Add(task);
            }

            var allRuleResults = await Task.WhenAll(tasks);

            if (allRuleResults.Any(r => r.AllowRequest == false))
                allRulesPass = false;
            return new AllowRequestResult(allRulesPass, failedReason);
        }
        catch (Exception ex)
        {
            allRulesPass = false;
            _logger.LogError(ex, "Rules processing was cancelled");
            throw;
        }

        
    }
}