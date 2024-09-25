using System.Collections.Generic;
using System.Threading.Tasks;

namespace RateLimiter.Processors;

/// <summary>
/// Represents a contextual rules processor that processes a collection of rules based on a context.
/// </summary>
/// <typeparam name="TRuleType">The type of rules to be processed.</typeparam>
/// <typeparam name="TProcessResult">The type of the processing result.</typeparam>
/// <typeparam name="TContext">The type of the context.</typeparam>
public interface IContextualRulesProcessor<TRuleType, TProcessResult, TContext>
{
    RulesCollection<TRuleType>? Ruleset { get; set; }

    IEnumerable<TContext> Context { get; set; }
    IContextualProcessingEngine<TRuleType, TProcessResult, TContext> Engine { get; set; }

    /// <summary>
    /// Processes the contextual rules based on the provided ruleset, context, and engine.
    /// </summary>
    /// <returns>A task representing the asynchronous operation that returns the result of the rule processing.</returns>
    Task<TProcessResult> ProcessRules();
}