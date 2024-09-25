using System.Collections.Generic;
using System.Threading.Tasks;

namespace RateLimiter.Processors;

/// Interface for a contextual processing engine.
/// This interface represents a processing engine for contextual rules. It provides a method to process rules
/// based on a specific context and returns the result of the processing.
/// @param <TRuleType> The type of the contextual rule
/// @param <TResult> The type of the result of processing the rules
/// @param <TContext> The type of the context for which the rules are applied
/// /
public interface IContextualProcessingEngine<in TRuleType, TResult, in TContext>
{
    /// <summary>
    /// Process the rate rules for a given context.
    /// </summary>
    /// <param name="rules">The rate rules to process.</param>
    /// <param name="context">The context for which the rules are applied.</param>
    /// <returns>An instance of AllowRequestResult indicating if all rules passed and the reason if any rule failed.</returns>
    Task<TResult> ProcessRules(IEnumerable<TRuleType> rules, IEnumerable<TContext>? context);
}