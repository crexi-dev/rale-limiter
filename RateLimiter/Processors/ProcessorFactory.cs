using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RateLimiter.Contracts;
using RateLimiter.Infrastructure;
using RateLimiter.Rules;
using Microsoft.Extensions.Logging;

namespace RateLimiter.Processors;

/// <summary>
/// The <c>ProcessorFactory</c> class is responsible for creating instances of processors based on the provided rules and context.
/// </summary>
public class ProcessorFactory : IProcessorFactory
{
    private readonly ILogger<ProcessorFactory> _logger;
    private readonly EngineFactory _engineFactory;

    public ProcessorFactory(ILogger<ProcessorFactory> logger, EngineFactory engineFactory)
    {
        _logger= logger;
        _engineFactory = engineFactory;
    }

    /// <summary>
    /// Gets a contextual processor based on the provided rules and context.
    /// </summary>
    /// <typeparam name="TReturnType">The type of the processor's return value.</typeparam>
    /// <typeparam name="TContextType">The type of the processor's context.</typeparam>
    /// <param name="rules">The rate rules to be applied by the processor.</param>
    /// <param name="rulesContext">The context to be passed to the processor.</param>
    /// <returns>A contextual processor that implements the <c>IContextualRulesProcessor</c> interface.</returns>
    public virtual async Task<IContextualRulesProcessor<IRateRule, TReturnType, TContextType>?> GetContextualProcessor<
        TReturnType, TContextType>(IEnumerable<IRateRule> rules, IEnumerable<RequestDetails>? rulesContext)
    {
        //TODO: Implement processor cache to get cache for same requests...
        return await Task.Run(() =>
        {
            try
            {
                var processingEngine = _engineFactory.GetEngine();
                var processor = new RateRulesProcessor(rules, processingEngine, rulesContext) as IContextualRulesProcessor<IRateRule, TReturnType, TContextType>;
                return processor;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error creating processor engine: {e.Message}");
                throw;
            }
        });
    }

    /// <summary>
    /// Gets the default ruleset based on the provided location.
    /// </summary>
    /// <param name="location">The location for which the default ruleset is retrieved.</param>
    /// <returns>The default ruleset as a list of <c>IRateRule</c> objects.</returns>
    public List<IRateRule> GetDefaultRuleset(string location)
    {
        List<IRateRule> result = location switch
        {
            "US" => new UsRuleSet(),
            "EUR" => new EuropeRuleset(),
            "RES" => new RestrictedRegionRuleset(),
            "PAC" => new PacificRegionRuleset(),
            _ => new DefaultRuleset()
        };

        return result;
    }
}