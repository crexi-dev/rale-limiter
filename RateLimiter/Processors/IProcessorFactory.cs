using System.Collections.Generic;
using System.Threading.Tasks;
using RateLimiter.Contracts;
using RateLimiter.Rules;

namespace RateLimiter.Processors;

public interface IProcessorFactory
{
    /// <summary>
    /// Gets a contextual processor based on the provided rules and context.
    /// </summary>
    /// <typeparam name="TReturnType">The type of the processor's return value.</typeparam>
    /// <typeparam name="TContextType">The type of the processor's context.</typeparam>
    /// <param name="rules">The rate rules to be applied by the processor.</param>
    /// <param name="rulesContext">The context to be passed to the processor.</param>
    /// <returns>A contextual processor that implements the <c>IContextualRulesProcessor</c> interface.</returns>
    Task<IContextualRulesProcessor<IRateRule, TReturnType, TContextType>?> GetContextualProcessor<
        TReturnType, TContextType>(IEnumerable<IRateRule> rules, IEnumerable<RequestDetails>? rulesContext);

    /// <summary>
    /// Gets the default ruleset based on the provided location.
    /// </summary>
    /// <param name="location">The location for which the default ruleset is retrieved.</param>
    /// <returns>The default ruleset as a list of <c>IRateRule</c> objects.</returns>
    List<IRateRule> GetDefaultRuleset(string location);
}